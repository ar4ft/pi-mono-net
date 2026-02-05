---
name: example-skill
description: An example skill demonstrating the Agent Skills format for the .NET coding agent.
---

# Example Skill

This is an example skill that demonstrates the proper format for creating skills in the Pi Coding Agent.

## Purpose

Skills provide specialized instructions that help the agent perform specific tasks more effectively. They are written in markdown with YAML frontmatter.

## Usage

When the agent detects a task that matches this skill's description, it should:

1. Read this skill file using the read tool
2. Follow the instructions contained within
3. Apply the specialized knowledge to complete the task

## Guidelines

- Keep skills focused on a specific task or domain
- Provide clear, actionable instructions
- Include examples when helpful
- Reference any tools or resources needed
- Use relative paths when referencing files in the skill directory

## Example

This skill can be used as a template for creating new skills. Simply:

1. Copy this SKILL.md file to a new directory in `.agents/skills/`
2. Update the frontmatter (name and description)
3. Write your specialized instructions
4. The agent will automatically discover and use it
