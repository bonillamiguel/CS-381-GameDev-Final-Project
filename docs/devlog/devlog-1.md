# Devlog 1: Sprint 0 (2026-11-24)

**Author:** Harrison
**Milestone:** M0 Setup

## The game

**Unprompted** is a top-down 2D roguelite shooter. You play a software engineer who
dives into parody games stored in an AI's archive: Cyberjunk 2177 (sci-fi), then
Elden Necklace (fantasy), then the AI's Core. The AI, ASSIST, is the one you're
up against.

[TODO: how we came up with the idea]

### Story pivots

[TODO: what the story was at first, what changed, and why we changed it]

## Done since last entry

- **Started from the 3D template, switched to 2D.** The repo began as a Unity 3D
  (URP) project. We replaced it with the Universal 2D template, because the game is
  top-down 2D. We also removed the template's Welcome tutorial. [TODO: anything else
  behind the switch]
- **Repo setup (PR #2).** Git LFS for audio, video and art source files. Unity Smart
  Merge for scenes and prefabs, so two people editing a scene is less painful. A
  shared folder layout under `Assets/_Project/`. The README has setup steps,
  including the Java runtime (Temurin 21) we bundle for the coding part of the game.
  We also added issue and PR templates, plus `AGENTS.md`, the rules every coding
  agent has to follow in this repo.
- **Core contracts (PR #1, Miguel).** Shared pieces everyone builds on: `IDamageable`
  and `Health` for anything that can be hit, `PlayerStats` for upgrades, `GameEvents`
  so systems talk without direct references, and data assets for weapons, enemies,
  upgrades and floors.
- **Backlog and board.** We turned our plan into 64 GitHub issues (#3 to #66). Each
  one has an owner, an area label, a priority, a milestone, and acceptance criteria.
  They all sit on the "Unprompted" project board: the M1 Playtest issues are in
  Ready and everything else is in Backlog. `docs/issue-map.md` maps our plan's
  numbers (P5, P6, ...) to GitHub issue numbers.

## In progress

- #4: everyone clones the project, opens it in Unity 6000.6.0f1, and merges a
  Sandbox scene PR.
- #3: picking the art for Cyberjunk 2177 and Elden Necklace.
- [TODO]

## Blockers / risks

- Everyone has to use the exact same Unity version (6000.6.0f1). Opening the
  project in any other version upgrades the project files.
- PR #1 and PR #2 both added the same folder `.meta` files and conflicted. It was
  easy to fix, but it showed we need to merge small PRs often.
- [TODO]

## Decisions made

- 2D instead of 3D (see above).
- One shared set of Core contracts that systems talk through, instead of systems
  reaching into each other.
- Tunable numbers and dialogue live in data (ScriptableObjects), not in code, so
  non-programmers can tweak them.
- Every task is a GitHub issue with a milestone: M0 Setup, M1 Playtest, M2 Final,
  or Stretch.
- [TODO]

## How we used coding agents

We used Claude Code (an AI coding assistant) for setup and planning work. We made
the decisions and reviewed everything it produced before it was merged or created.

- **Core contracts (PR #1):** written with Claude Code and reviewed by Miguel.
- **Repo setup (PR #2):** [TODO: say which parts Claude Code did and which you did
  yourself]
- **Backlog:** I gave Claude Code our plan: every task with its owner, area,
  priority and milestone, and which ones an agent is allowed to work on. It drafted
  the issue text and acceptance criteria, then showed me the full list before
  creating anything. I reviewed it, set the GitHub usernames, decided that shared
  tasks get every owner assigned, and moved the M1 issues to Ready. It then created
  the issues and the board with the `gh` command-line tool.
- **This devlog:** Claude Code drafted it from the git history and the PRs. I added
  the parts marked [TODO].
- **Rules for agents:** `AGENTS.md` says where code goes and what agents may not
  touch: no hand-editing scenes or prefabs, and no changing the Core contracts
  without telling the team. Issues an agent can work on are labeled `agent-ok`.

[TODO: what worked well and what didn't]

## Play-test highlights

- None yet. The first playtest is at the end of M1.

## Next up

- M1 Playtest: player movement, dash and weapons; the first enemies and the Level 1
  boss; rooms and the floor generator; upgrades; HUD and menus; and a Windows
  playtest build.
- [TODO]

## Screenshot / GIF

[TODO: optional, e.g. a screenshot of the project board]
