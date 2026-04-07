# Contributing <img src="https://avatars.githubusercontent.com/u/115114649?s=400&u=7a327b1aaf9190fe648d5f834b87a16a8b13ae89&v=4" alt="The Noobs" style="width:25px;height:25px;">

First off, thanks for taking the time to contribute!

## How can I contribute?

- Fork this project;
- Make your changes / new implementations;
- Use the pattern for git commits;
- Make sure that the acceptance criteria are met (tests, docs, etc);
- Follow the code conventions;
- Create a pull request;

## Pull Requests

Template [PULL REQUEST TEMPLATE](PULL_REQUEST_TEMPLATE.md)

## Git Commit Messages

This project follows the [Conventional Commits](https://www.conventionalcommits.org/) specification.

### Format

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### Types

| Type | Description |
|------|-------------|
| `feat` | A new feature |
| `fix` | A bug fix |
| `docs` | Documentation only changes |
| `style` | Changes that do not affect the meaning of the code (formatting, semicolons, etc.) |
| `refactor` | A code change that neither fixes a bug nor adds a feature |
| `perf` | A code change that improves performance |
| `test` | Adding missing tests or correcting existing tests |
| `build` | Changes that affect the build system or external dependencies |
| `ci` | Changes to CI configuration files and scripts |
| `chore` | Other changes that don't modify src or test files |

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

### Guidelines

- Use the present tense ("add feature" not "added feature")
- Limit the first line to 72 characters or less
- Reference issues and pull requests liberally
