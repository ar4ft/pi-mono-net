namespace Pi.CodingAgent.Skills;

/// <summary>
/// Loads skills from directories and files.
/// Supports recursive directory scanning and multi-location skill discovery.
/// </summary>
public static class SkillLoader
{
    /// <summary>
    /// Load skills from a directory.
    ///
    /// Discovery rules:
    /// - direct .md children in the root
    /// - recursive SKILL.md under subdirectories
    /// </summary>
    public static LoadSkillsResult LoadSkillsFromDir(string dir, string source)
    {
        return LoadSkillsFromDirInternal(dir, source, includeRootFiles: true);
    }

    private static LoadSkillsResult LoadSkillsFromDirInternal(string dir, string source, bool includeRootFiles)
    {
        var skills = new List<Skill>();
        var diagnostics = new List<ResourceDiagnostic>();

        if (!Directory.Exists(dir))
        {
            return new LoadSkillsResult { Skills = skills, Diagnostics = diagnostics };
        }

        try
        {
            var entries = Directory.GetFileSystemEntries(dir);

            foreach (var entryPath in entries)
            {
                var entryName = Path.GetFileName(entryPath);

                // Skip hidden files and node_modules
                if (entryName.StartsWith(".") || entryName == "node_modules")
                {
                    continue;
                }

                // Handle symlinks and check if they're directories or files
                FileAttributes attrs;
                try
                {
                    attrs = File.GetAttributes(entryPath);
                }
                catch
                {
                    // Broken symlink or inaccessible, skip it
                    continue;
                }

                bool isDirectory = (attrs & FileAttributes.Directory) == FileAttributes.Directory;

                if (isDirectory)
                {
                    var subResult = LoadSkillsFromDirInternal(entryPath, source, includeRootFiles: false);
                    skills.AddRange(subResult.Skills);
                    diagnostics.AddRange(subResult.Diagnostics);
                    continue;
                }

                // Check if it's a valid skill file
                bool isRootMd = includeRootFiles && entryName.EndsWith(".md");
                bool isSkillMd = !includeRootFiles && entryName == "SKILL.md";

                if (!isRootMd && !isSkillMd)
                {
                    continue;
                }

                var result = LoadSkillFromFile(entryPath, source);
                if (result.Skill != null)
                {
                    skills.Add(result.Skill);
                }
                diagnostics.AddRange(result.Diagnostics);
            }
        }
        catch
        {
            // Silently ignore directory access errors
        }

        return new LoadSkillsResult { Skills = skills, Diagnostics = diagnostics };
    }

    private static (Skill? Skill, List<ResourceDiagnostic> Diagnostics) LoadSkillFromFile(string filePath, string source)
    {
        var diagnostics = new List<ResourceDiagnostic>();

        try
        {
            var rawContent = File.ReadAllText(filePath);
            var (frontmatter, _) = FrontmatterParser.ParseFrontmatter(rawContent);
            var allKeys = FrontmatterParser.GetFrontmatterKeys(rawContent);
            var skillDir = Path.GetDirectoryName(filePath) ?? "";
            var parentDirName = Path.GetFileName(skillDir);

            // Validate frontmatter fields
            var fieldErrors = SkillValidator.ValidateFrontmatterFields(allKeys);
            foreach (var error in fieldErrors)
            {
                diagnostics.Add(new ResourceDiagnostic
                {
                    Type = "warning",
                    Message = error,
                    Path = filePath
                });
            }

            // Validate description
            var descErrors = SkillValidator.ValidateDescription(frontmatter.Description);
            foreach (var error in descErrors)
            {
                diagnostics.Add(new ResourceDiagnostic
                {
                    Type = "warning",
                    Message = error,
                    Path = filePath
                });
            }

            // Use name from frontmatter, or fall back to parent directory name
            var name = frontmatter.Name ?? parentDirName;

            // Validate name
            var nameErrors = SkillValidator.ValidateName(name, parentDirName);
            foreach (var error in nameErrors)
            {
                diagnostics.Add(new ResourceDiagnostic
                {
                    Type = "warning",
                    Message = error,
                    Path = filePath
                });
            }

            // Still load the skill even with warnings (unless description is completely missing)
            if (string.IsNullOrWhiteSpace(frontmatter.Description))
            {
                return (null, diagnostics);
            }

            return (new Skill
            {
                Name = name,
                Description = frontmatter.Description,
                FilePath = filePath,
                BaseDir = skillDir,
                Source = source,
                DisableModelInvocation = frontmatter.DisableModelInvocation ?? false
            }, diagnostics);
        }
        catch (Exception error)
        {
            var message = error.Message ?? "failed to parse skill file";
            diagnostics.Add(new ResourceDiagnostic
            {
                Type = "warning",
                Message = message,
                Path = filePath
            });
            return (null, diagnostics);
        }
    }

    /// <summary>
    /// Load skills from all configured locations.
    /// Returns skills and any validation diagnostics.
    /// </summary>
    public static LoadSkillsResult LoadSkills(LoadSkillsOptions? options = null)
    {
        options ??= new LoadSkillsOptions();
        
        var cwd = options.Cwd ?? Directory.GetCurrentDirectory();
        var agentDir = options.AgentDir ?? GetAgentDir();
        var skillPaths = options.SkillPaths ?? new List<string>();
        var includeDefaults = options.IncludeDefaults ?? true;

        var skillMap = new Dictionary<string, Skill>();
        var realPathSet = new HashSet<string>();
        var allDiagnostics = new List<ResourceDiagnostic>();
        var collisionDiagnostics = new List<ResourceDiagnostic>();

        void AddSkills(LoadSkillsResult result)
        {
            allDiagnostics.AddRange(result.Diagnostics);
            foreach (var skill in result.Skills)
            {
                // Resolve symlinks to detect duplicate files
                string realPath;
                try
                {
                    realPath = Path.GetFullPath(skill.FilePath);
                }
                catch
                {
                    realPath = skill.FilePath;
                }

                // Skip silently if we've already loaded this exact file (via symlink)
                if (realPathSet.Contains(realPath))
                {
                    continue;
                }

                if (skillMap.TryGetValue(skill.Name, out var existing))
                {
                    collisionDiagnostics.Add(new ResourceDiagnostic
                    {
                        Type = "collision",
                        Message = $"name \"{skill.Name}\" collision",
                        Path = skill.FilePath,
                        Collision = new SkillCollision
                        {
                            ResourceType = "skill",
                            Name = skill.Name,
                            WinnerPath = existing.FilePath,
                            LoserPath = skill.FilePath
                        }
                    });
                }
                else
                {
                    skillMap[skill.Name] = skill;
                    realPathSet.Add(realPath);
                }
            }
        }

        if (includeDefaults)
        {
            // Load from user directory ~/.agents/skills
            AddSkills(LoadSkillsFromDirInternal(Path.Combine(agentDir, "skills"), "user", true));
            // Load from project directory .agents/skills
            AddSkills(LoadSkillsFromDirInternal(Path.Combine(cwd, ".agents", "skills"), "project", true));
        }

        var userSkillsDir = Path.Combine(agentDir, "skills");
        var projectSkillsDir = Path.Combine(cwd, ".agents", "skills");

        bool IsUnderPath(string target, string root)
        {
            var normalizedRoot = Path.GetFullPath(root);
            var normalizedTarget = Path.GetFullPath(target);
            return normalizedTarget.StartsWith(normalizedRoot + Path.DirectorySeparatorChar) || normalizedTarget == normalizedRoot;
        }

        string GetSource(string resolvedPath)
        {
            if (!includeDefaults)
            {
                if (IsUnderPath(resolvedPath, userSkillsDir)) return "user";
                if (IsUnderPath(resolvedPath, projectSkillsDir)) return "project";
            }
            return "path";
        }

        foreach (var rawPath in skillPaths)
        {
            var resolvedPath = ResolveSkillPath(rawPath, cwd);
            if (!File.Exists(resolvedPath) && !Directory.Exists(resolvedPath))
            {
                allDiagnostics.Add(new ResourceDiagnostic
                {
                    Type = "warning",
                    Message = "skill path does not exist",
                    Path = resolvedPath
                });
                continue;
            }

            try
            {
                var source = GetSource(resolvedPath);
                if (Directory.Exists(resolvedPath))
                {
                    AddSkills(LoadSkillsFromDirInternal(resolvedPath, source, true));
                }
                else if (File.Exists(resolvedPath) && resolvedPath.EndsWith(".md"))
                {
                    var (skill, diagnostics) = LoadSkillFromFile(resolvedPath, source);
                    if (skill != null)
                    {
                        AddSkills(new LoadSkillsResult { Skills = new List<Skill> { skill }, Diagnostics = diagnostics });
                    }
                    else
                    {
                        allDiagnostics.AddRange(diagnostics);
                    }
                }
                else
                {
                    allDiagnostics.Add(new ResourceDiagnostic
                    {
                        Type = "warning",
                        Message = "skill path is not a markdown file",
                        Path = resolvedPath
                    });
                }
            }
            catch (Exception error)
            {
                var message = error.Message ?? "failed to read skill path";
                allDiagnostics.Add(new ResourceDiagnostic
                {
                    Type = "warning",
                    Message = message,
                    Path = resolvedPath
                });
            }
        }

        return new LoadSkillsResult
        {
            Skills = skillMap.Values.ToList(),
            Diagnostics = allDiagnostics.Concat(collisionDiagnostics).ToList()
        };
    }

    private static string NormalizePath(string input)
    {
        var trimmed = input.Trim();
        if (trimmed == "~")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        if (trimmed.StartsWith("~/") || trimmed.StartsWith("~\\"))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), trimmed.Substring(2));
        }
        return trimmed;
    }

    private static string ResolveSkillPath(string p, string cwd)
    {
        var normalized = NormalizePath(p);
        return Path.IsPathRooted(normalized) ? normalized : Path.Combine(cwd, normalized);
    }

    private static string GetAgentDir()
    {
        // Default to ~/.agents
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".agents");
    }
}

/// <summary>
/// Options for loading skills
/// </summary>
public record LoadSkillsOptions
{
    /// <summary>
    /// Working directory for project-local skills. Default: current directory
    /// </summary>
    public string? Cwd { get; init; }

    /// <summary>
    /// Agent config directory for global skills. Default: ~/.agents
    /// </summary>
    public string? AgentDir { get; init; }

    /// <summary>
    /// Explicit skill paths (files or directories)
    /// </summary>
    public List<string>? SkillPaths { get; init; }

    /// <summary>
    /// Include default skills directories. Default: true
    /// </summary>
    public bool? IncludeDefaults { get; init; }
}
