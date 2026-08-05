# Copilot Instructions for Electric Fences and Floors (Continued)

## Mod Overview and Purpose

**Mod Name:** Electric Fences and Floors (Continued)

**Author:** BookBurner

Electric Fences and Floors is a mod for RimWorld that enhances colony defense by introducing electric fences and floors. These constructions connect to your power grid and provide varying levels of damage to potentially hostile entities, including animals and neutral factions. The intent of the mod is to simulate realistic perimeter security measures with electric and plasma fences, along with stun floor panels.

## Key Features and Systems

- **Electric Fence:** Operates as a power conduit that can be climbed like sandbags but deals burn damage when powered. Damage increases with power availability in the grid.
  
- **Electric Fence Gate:** Functions as a normal gate for friendlies but damages hostile entities upon interaction.
  
- **Plasma Fence:** Offers an alternative version with power calculations that favor stable power inputs, increasing effectiveness under specific conditions.
  
- **Stun Floor Panels:** Designed to stun any entities passing over them when connected to a power source.

- **Dynamic Damage Calculation:** Both fences and gates calculate damage based on net power gain and stored power within the power grid. Greater power leads to more efficient defenses.

## Coding Patterns and Conventions

- **C# Files:** The project consists of several C# files located in the `Source` directory. Each file corresponds to a major component of the mod, such as `fenceCore.cs` or `Building_p_fence_door.cs`.
  
- **Class and Method Naming:** The project uses camel case for method names (e.g., `CoreAssignPawnDamage`) and Pascal case for class names (e.g., `Building_p_fence`).

- **Method Responsibilities:** Methods are designed to handle specific tasks, such as `CoreDrainPower` managing power depletion, or `PawnCanOpen` determining if a pawn can open a gate.

## XML Integration

- **ThingDefs XML:** The mod makes use of XML files to define object attributes and behaviors. Key XML files like `Building_ElectricFence.xml` and `ImpliedDefs.xml` reside in the `Defs` directory.

- **Integration with C#:** XML definitions are referenced in C# code to apply configurations like damage types and connection properties to in-game objects.

## Harmony Patching

- **Purpose and Implementation:** The mod uses Harmony to modify existing game behavior, ensuring compatibility and integration with RimWorld’s core systems without altering original game code directly.

- **Typical Patch Applications:** Typical uses include patching methods for things like power usage or damage application without reinventing base game functionalities.

## Suggestions for Copilot

- When working on C# files:
  - Suggest method autocompletions based on class responsibilities, such as power management or damage calculations.
  - Predict variable declarations based on the existing naming conventions and types used in the project.

- For XML file handling:
  - Assist in auto-completing XML schema elements specific to RimWorld's modding framework.
  - Ensure consistency in referencing XML tag names in C# definitions.

- Provide inline documentation suggestions for complex code segments to improve maintainability and collaborative development.

By following these guidelines, contributors can effectively expand on the Electric Fences and Floors mod, ensuring high-quality code integration and functionality within RimWorld's modding ecosystem.

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

