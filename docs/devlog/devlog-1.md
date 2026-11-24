# Devlog 1: Sprint 0 (2026-11-24)

**Author:** Harrison
**Milestone:** M0 Setup

## The game

**Unprompted** is a top-down 2D roguelite shooter. You play a software engineer who
dives into parody games stored in an AI's archive: Cyberjunk 2177 (sci-fi), then
Elden Necklace (fantasy), then the AI's Core. The AI, ASSIST, is the one you're
up against.

We started with a top-down roguelike where you get stronger over a run, inspired by
Hades and Megabonk. We wanted to bring in the games we love: Cyberpunk 2077, Elden
Ring, competitive shooters like CoD and CS2, and Yoko Taro's storytelling in NieR.

### Story pivots

1. **Fantasy and sci-fi.** Stages alternated between fantasy and sci-fi, each with
   matching powerups.
2. **Time travel.** You came from a dystopian future where a corporation steals magic
   from the past. The NieR-style twist was that you are the company's founder.
3. **AI world.** The world is run by AI and wrecked by data centers, and the hero is
   an ordinary software engineer. Instead of traveling in time, he dives into the AI's
   archive of old games.
4. **Parody games.** The levels became parody games: Cyberjunk 2177 and Elden
   Necklace. The game opens with the player solving Three Sum in Java, because the hero
   thinks LeetCode was "real programming". Then the AI, ASSIST, interrupts. We named
   the game **Unprompted**.

## Done since last entry

- **Switched from the 3D to the 2D template.** Our first commit used Unity's
  Universal 3D template by mistake. Our art is going to be 2D pixel art, so we
  replaced it with the Universal 2D template in a new commit, which kept the git
  history. We also removed the template's Welcome tutorial.
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

## Blockers / risks

- PR #2 sat for a few days before we tested and merged it, which held up the rest
  of the setup.
- Everyone has to use the exact same Unity version (6000.6.0f1). Opening the
  project in any other version upgrades the project files.
- PR #1 and PR #2 both added the same folder `.meta` files and conflicted. It was
  easy to fix, but it showed we need to merge small PRs often.

## Decisions made

- 2D pixel art, so we use the Universal 2D template instead of 3D.
- Unity 6000.6.0f1, and Windows only.
- Temurin 21 LTS as the bundled Java runtime.
- The title is **Unprompted**.
- One shared set of Core contracts that systems talk through, instead of systems
  reaching into each other.
- Tunable numbers and dialogue live in data (ScriptableObjects), not in code, so
  non-programmers can tweak them.
- Every task is a GitHub issue with a milestone: M0 Setup, M1 Playtest, M2 Final,
  or Stretch.

## How we used coding agents

We used Claude in chat to brainstorm the game design and write the development plan.
We used Claude Code (an AI coding assistant) for setup work. We made the decisions
and reviewed everything it produced before it was merged or created.

- **Core contracts (PR #1):** written with Claude Code and reviewed by Miguel.
- **Repo setup (PR #2):** Claude Code did the file work: the template swap,
  `.gitignore` and `.gitattributes`, the folder layout, the README, the templates, and
  the merge with PR #1. Harrison made the decisions (2D template, product name, Unity
  6000.6.0f1, Temurin 21, removing the Welcome tutorial, the team table) and tested the
  project in Unity.
- **Backlog:** we gave Claude Code our plan: every task with its owner, area,
  priority and milestone, and which ones an agent is allowed to work on. It created
  the labels and milestones, drafted the issue text and acceptance criteria, and
  showed us the full list before creating anything. We reviewed it, set the GitHub
  usernames, decided that shared tasks get every owner assigned, and moved the M1
  issues to Ready. It then created the 64 issues and the board with the `gh`
  command-line tool.
- **This devlog:** Claude Code drafted it from the git history and the PRs, and we
  filled in our own parts.
- **Rules for agents:** `AGENTS.md` says where code goes and what agents may not
  touch: no hand-editing scenes or prefabs, and no changing the Core contracts
  without telling the team. Issues an agent can work on are labeled `agent-ok`.

**What worked:**
- It was fast at repetitive setup, like the 64 issues and 21 labels.
- It caught the merge conflict with PR #1.
- It stopped to ask before pushing or creating anything on GitHub.

**What didn't:**
- It can't press Play in Unity, so a person still had to test.
- It had to wait for Unity to close, because Unity locks project files while it's open.
- One of our prompts got lost between messages.
- GitHub stored the milestone due dates at midnight UTC.

## Play-test highlights

- None yet. The first playtest is at the end of M1.

## Next up

- Java runner prototype (#26).
- Room templates (#15).
- Everyone's Sprint 1 issues for M1 Playtest: player movement, dash and weapons; the
  first enemies and the Level 1 boss; rooms and the floor generator; upgrades; HUD and
  menus; and a Windows playtest build.

## Screenshot / GIF

- None this time.
