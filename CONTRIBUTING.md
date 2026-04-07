# Contributing to RestSharp.Authenticators.Digest

Thank you for your interest in contributing to this project!
Whether you're fixing a bug, adding a feature, improving documentation, or reporting an issue — your help is
appreciated.

---

## 🧾 Table of Contents

- [How to contribute](#-how-to-contribute)
- [Issue reporting](#-issue-reporting)
- [Pull request guidelines](#-pull-request-guidelines)
- [Development setup](#-development-setup)
- [Commit message convention](#-commit-message-convention)
- [Code style and conventions](#-code-style-and-conventions)
- [License](#-license)

---

## ✅ How to contribute

There are several ways to contribute:

- 🐛 Report a bug
- ✨ Propose or implement a feature
- 📝 Improve documentation
- 🔧 Refactor or optimize existing code
- ✅ Add tests or improve coverage

---

## 🐞 Issue reporting

If you've encountered a bug or unexpected behavior:

1. Search [existing issues](https://github.com/thenoobsbr/RestSharp.Authenticators.Digest/issues) to avoid duplicates.
2. If none exists, open a **new issue** with:
    - Steps to reproduce
    - Expected behavior
    - Environment (.NET version, OS, etc.)
    - Error logs or test case (if available)

Clear, reproducible issues help us fix things faster.

---

## 🚀 Pull request guidelines

To contribute code:

1. Fork the repository
2. Create a new branch:
    ```bash
       git checkout -b feature/my-feature
    ```
3. Write your code and tests
4. Run tests locally:
    ```bash
      dotnet test
    ```
5. Commit using **Conventional Commits** (see below)
6. Push to your fork and open a PR against `main`

### PR checklist:

* ✅ Your code builds and tests pass
* ✅ Follows the coding style (see below)
* ✅ If you add functionality, include corresponding tests
* ✅ Commit messages follow Conventional Commits
* ✅ Reference related issues in the PR (e.g., `Fixes #12`)

---

## 🧪 Development setup

You need:

* [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
* (Optionally) [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
* Git, an IDE (like Visual Studio or VS Code), and NuGet

Clone the repository:

```bash
git clone https://github.com/thenoobsbr/RestSharp.Authenticators.Digest.git
cd RestSharp.Authenticators.Digest
```

Run tests:

```bash
dotnet test
```

---

## 📝 Commit message convention

This project follows the [Conventional Commits](https://www.conventionalcommits.org/) specification.

### Format

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### Types

| Type       | Description                                                                       |
|------------|-----------------------------------------------------------------------------------|
| `feat`     | A new feature                                                                     |
| `fix`      | A bug fix                                                                         |
| `docs`     | Documentation only changes                                                        |
| `style`    | Changes that do not affect the meaning of the code (formatting, semicolons, etc.) |
| `refactor` | A code change that neither fixes a bug nor adds a feature                         |
| `perf`     | A code change that improves performance                                           |
| `test`     | Adding missing tests or correcting existing tests                                 |
| `build`    | Changes that affect the build system or external dependencies                     |
| `ci`       | Changes to CI configuration files and scripts                                     |
| `chore`    | Other changes that don't modify src or test files                                 |

### Examples

```
feat(auth): add CancellationToken support to Authenticate method
fix(header): handle unquoted qop values in WWW-Authenticate header
test(integration): add tests for wrong credentials scenario
docs: update CONTRIBUTING with conventional commits
build(deps): upgrade RestSharp to 114.0.0
```

### Breaking changes

Append `!` after the type/scope or add a `BREAKING CHANGE:` footer:

```
feat(auth)!: require CancellationToken in IAuthenticator.Authenticate

BREAKING CHANGE: Authenticate method signature now includes CancellationToken parameter.
```

---

## 🎯 Code style and conventions

*
Use [C# standard conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
* Prefer `async/await` over `Task.Result` or `.Wait()`
* Use explicit access modifiers (`public`, `private`, etc.)
* Tests follow the `Given_When_Then` pattern where appropriate

We recommend enabling automatic formatting with:

```bash
dotnet format
```

---

## 📄 License

By contributing, you agree that your code will be licensed under the [MIT License](LICENSE).
