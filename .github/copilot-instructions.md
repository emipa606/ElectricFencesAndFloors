# GitHub Copilot Instructions for Electric Fences and Floors (Continued) Mod

Welcome to the Electric Fences and Floors (Continued) mod for RimWorld. This mod aims to bring electric fences and plasma fence systems to the game, providing a more dynamic and deadly perimeter defense option.

## Mod Overview and Purpose

The Electric Fences and Floors (Continued) mod aims to add electric fence and plasma fence systems to RimWorld. These fences work as power conduits that can be climbed but do burn damage to anything potentially hostile that might try to pass through them when connected to the power grid. The more power stored in the grid, the more efficient the fence is. The mod also includes a stun floor panel that can be used to incapacitate enemies.

## Key Features and Systems

- **Electric Fence**: A power conduit that can be climbed but does burn damage to anything potentially hostile.
- **Electric Fence Gate**: A gate that can be opened by anyone but that does damage to potentially hostile factions.
- **Plasma Fence**: A high-power fence that favors large stable power gains.
- **Stun Floor Panels**: A floor panel that can be used to incapacitate enemies.

## Coding Patterns and Conventions

- **Mod Structure**: The mod is structured with separate C# files for different building types and core functionality.
- **Naming Conventions**: Use clear and descriptive names for classes, methods, and variables.
- **Code Organization**: Organize code into logical sections and use comments to explain complex logic.

## XML Integration

- **Defs Files**: The mod uses XML files to define new game elements such as ThingDefs and ImpliedDefs.
- **Def Structure**: Follow the RimWorld def structure and use the provided XML files as templates.

## Harmony Patching

- **Harmony Patching**: Use Harmony patches to modify existing game behavior without altering the original game files.
- **Patch Location**: Place harmony patches in the appropriate C# files.
- **Patch Structure**: Follow the Harmony patching pattern and ensure that patches are correctly named and organized.

## Suggestions for Copilot

1. **Feature Request**: Ask Copilot to generate new features for the mod, such as additional fence types or new mechanics.
2. **Code Optimization**: Ask Copilot to optimize existing code for performance and readability.
3. **Bug Fixing**: Ask Copilot to help identify and fix bugs in the mod.
4. **Documentation**: Ask Copilot to generate documentation for new features or updated code.

## Example Usage

- **Feature Request**: "Can you generate a new type of fence that uses EMP technology?"
- **Code Optimization**: "Can you optimize the damage calculation logic for the electric fence?"
- **Bug Fixing**: "Can you help identify and fix the issue with the electric fence gate not opening properly?"
- **Documentation**: "Can you generate documentation for the new plasma fence system?"

## Support

For best support, please use the Discord-channel for error-reporting. Do not report errors by making a discussion-thread, as you won't receive any notification. If you have a solution for a problem, please post it to the GitHub repository. Use RimSort to sort your mods.

## License

This mod is licensed under the MIT License. You are free to use, modify, and distribute the mod as long as you include the original license and copyright notice.

## Contact

If you have any questions or suggestions for the mod, please feel free to contact me via the GitHub repository.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

