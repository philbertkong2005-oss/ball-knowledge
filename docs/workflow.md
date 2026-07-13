# Ball Knowledge Workflow

This file is the working contract for every future Codex session. If a step below is skipped, the task is not done yet.

## 1. Verification Has Two Levels

Use the right proof for the kind of change you made.

- Gameplay or UI change: verify by playing the game and watching the result with your own eyes.
- Non-visual change: finish with a command you can run yourself, plus a log or artifact whose expected result is written down before the change is made.

Copy-paste examples:

```powershell
.venv\Scripts\python tools\validate_constants.py
```

Expected result: the command exits with code `0` and prints that the constants file passed validation.

```powershell
.venv\Scripts\pre-commit run --all-files
```

Expected result: every hook prints `Passed`.

## 2. Every Feature Includes Tests

Codex should add or update automated tests with every feature. A feature is not done until the matching tests run and pass.

If the feature adds Python logic, the command should be written down in the task before coding starts. Example:

```powershell
.venv\Scripts\python -m unittest discover -s tests -p "test*.py"
```

Expected result: all tests pass, with `OK` at the end.

## 3. Commit Discipline

Keep one feature per commit. If Unity or package files change for mechanical reasons, that can be a separate mechanical commit so the real feature stays easy to roll back.

After a verified good state, create a tag as a rollback anchor.

Copy-paste command:

```powershell
git tag good-YYYYMMDD-HHMM
```

Example:

```powershell
git tag good-20260713-2130
```

Check your anchors any time:

```powershell
git tag --list "good-*"
```

## 4. Safe Rollback Cheat-Sheet

Use the smallest safe recovery path that matches the mistake.

### A. Unpushed Local Mistake

If you changed files locally and have not pushed anything yet, first inspect the state:

```powershell
git status
```

To throw away changes in one file and restore the last committed version:

```powershell
git restore path\to\file
```

To throw away all local uncommitted changes:

```powershell
git restore .
```

To jump all the way back to a known-good tag when the bad work is still unpushed:

```powershell
git reset --hard good-YYYYMMDD-HHMM
```

Check the result:

```powershell
git status
git log --oneline -5
```

### B. Pushed Mistake (Default Recovery Path)

If the bad commit was already pushed, do not rewrite history as your normal path. Revert it with a new commit instead.

Find the bad commit:

```powershell
git log --oneline -10
```

Create a revert commit:

```powershell
git revert <bad-commit-sha>
```

Then push manually after you review the result:

```powershell
git push
```

### C. Expert-Only History Rewrite

This is not the beginner path. Only use it if you fully understand the risk and you specifically want to rewrite remote history.

```powershell
git push --force-with-lease
```

If you are unsure, do not use that command. Use `git revert <sha>` instead.

## 5. Secrets Policy

Never put real secrets in this repo. Never paste real secrets into AI prompts.

- `.env` is ignored by git and can hold local machine-only values if needed later.
- Do not commit API keys, tokens, passwords, private URLs, or email credentials.
- `git push` is always a manual human step after local checks pass.

Quick safety check before a commit or push:

```powershell
git status
git diff -- . ':!.env'
```

## 6. Session Size

Size work so one session fits inside `1` to `2` hours. That keeps the project compatible with a `5` to `15` hour week and makes rollback easier if something goes wrong.
