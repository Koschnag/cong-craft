# Contributing to CongCraft

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

### Setup
```bash
git clone https://github.com/Koschnag/cong-craft.git
cd cong-craft
dotnet restore
dotnet build
dotnet test
```

### Run the Game
```bash
dotnet run --project src/CongCraft.Game
```

## Development Workflow

### 1. Pick an Issue
- Check [open issues](https://github.com/Koschnag/cong-craft/issues)
- Look for `good first issue` or `help wanted` labels
- Comment on the issue to claim it

### 2. Create a Branch
```bash
git checkout -b feature/your-feature-name
```

### 3. Make Changes
- Follow the project structure in `CLAUDE.md`
- Write tests for new functionality
- Keep changes focused and atomic

### 4. Test
```bash
dotnet test                    # All tests must pass
dotnet build --configuration Release  # Must build cleanly
```

### 5. Open a Pull Request
- Use the PR template
- Link related issues
- Describe what changed and why
- Add screenshots for visual changes

### 6. Review Process
- CI runs automatically (build, test, coverage)
- A pre-release build is created for testing
- Maintainer reviews and provides feedback
- Once approved, PR is merged

## Code Guidelines

### Architecture
- Follow ECS patterns - components hold data, systems hold logic
- Keep systems focused on a single responsibility
- Use `ServiceLocator` for cross-system dependencies

### AOT Compatibility
All code must be Native AOT compatible:
- No `System.Reflection` for serialization
- Use `System.Text.Json` source generators
- No dynamic code generation

### Testing
- Write xUnit tests for all new systems and components
- Test files go in `tests/CongCraft.Engine.Tests/` mirroring the source structure
- Use descriptive test method names: `MethodName_Scenario_ExpectedResult`

## Issue Templates

When creating issues, use the appropriate template:
- **Bug Report**: For crashes, glitches, or broken features
- **Feature Request**: For new gameplay features or improvements
- **Task**: For development tasks, refactoring, or chores
- **Epic**: For large features that span multiple tasks

## Release Process

Releases are fully automated:
1. Merge PRs to `main`
2. Create a version tag: `git tag v1.0.0 && git push origin v1.0.0`
3. GitHub Actions builds for all platforms and creates the release
4. Pre-releases are automatically created for open PRs
