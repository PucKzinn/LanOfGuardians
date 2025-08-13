# AGENTS.md — Diretrizes para o Codex

> Projeto: **Unity 2021.3** (C#) · **Mirror** (rede) · **SQLite4Unity3d** (persistência) Plataforma alvo inicial: Windows (Editor/Standalone, x86\_64)

## Objetivo do agente

Automatizar refactors, correções, organização e documentação dos **scripts C#** do projeto, abrindo Pull Requests pequenos, claros e testáveis.

## Áreas que o agente PODE alterar

* `Assets/Scripts/**` (C#)
* `Assets/Tests/**` (testes unitários de lógica pura)
* `README.md`, `AGENTS.md`, `CHANGELOG.md`
* Arquivos de configuração de lint/format (`.editorconfig`, `Directory.Build.props`, etc., se adicionados)

## Áreas que o agente NÃO deve alterar

* `Assets/Scenes/**` (cenas)
* `Assets/Prefabs/**` (prefabs)
* `Assets/**.unity`, `**.prefab`, `**.asset`, `**.mat` (a menos que explicitamente solicitado)
* `Assets/Plugins/**` (Mirror, SQLite nativos/DLLs)
* `ProjectSettings/**`, `Packages/**`

> Motivo: o ambiente do Codex **não executa o Unity Editor**. Alterações em cenas/prefabs podem quebrar referências sem verificação.

## Padrões de código

* C# 8+; nomeação clara; métodos curtos; early-return; SOLID pragmático.
* Evitar alocações/GC em loops de rede (Mirror). Use `readonly`/`struct` quando apropriado.
* Logs: usar `Debug.unityLogger?.Log(LogType.X, "[Tag] mensagem")` ou `Debug.Log*` com tags curtas.
* Validar parâmetros públicos (`ArgumentNullException`, `ArgumentOutOfRangeException`).
* Comentários XML nos serviços (`AccountService`, `CharacterService`) e classes públicas reutilizáveis.

## Tarefas típicas para o agente

1. **Refactor**: reduzir duplicação, extrair métodos, aplicar `readonly`, imutabilidade onde útil.
2. **Segurança básica**: validar input de login; limitar tentativas; normalizar nomes de usuário.
3. **Erros comuns Mirror**: atualizar para `NetworkMessage` (sem `MessageBase`), checagens de `NetworkIdentity` e nulls.
4. **SQLite4Unity3d**: envolver acesso a DB em camada única (`Database.Conn`), adicionar utilidades de migração simples.
5. **Testes unitários** (lógica pura): hashing de senha, validação de login, serialização de mensagens.
6. **Documentação**: README com instruções de build/teste; changelog incremental de PRs.

## Expectativas de Pull Requests

* **Escopo pequeno** (≤ \~300 linhas alteradas quando possível).
* Descrição clara: problema, solução, impacto, como testar.
* Sem alterações em cenas/prefabs/plugins, salvo pedido explícito.
* Passar lints/compilação C# (se houver pipeline).

## Layout relevante do projeto

```
Assets/
  Scripts/
    Server/
      DB/ (Database.cs, AccountService.cs, CharacterService.cs)
      SimpleAuthenticator.cs
      PlayerSpawner.cs
    Shared/
      PlayerNetwork.cs
    Client/
      LoginUI.cs
  Plugins/
    SQLite/ (SQLite.cs)
    x86_64/ (sqlite3.dll)
  Scenes/ (ServerScene.unity, GameScene.unity)
```

## Dependências e restrições

* **Mirror**: versão manual em `Assets/Mirror/**`. Usar `NetworkMessage`.
* **SQLite**: via **SQLite4Unity3d** (`SQLite.cs`) + `sqlite3.dll` x86\_64.
* **Unity**: `Api Compatibility Level = .NET Framework`; `Scripting Backend = Mono`.

## Como o agente deve proceder

1. Fazer **scan** em `Assets/Scripts/**` e abrir **issues/notas** sobre problemas comuns (null refs, logs ausentes, duplicações).
2. Propor PRs iterativos (1 tema por PR) com melhorias incrementais.
3. Adicionar testes **NUnit** apenas para lógica que **não** depende de `UnityEngine`/Editor.
4. Incluir **Checklist de teste manual** na descrição do PR quando relevante (ex.: login/spawn).

## Exemplos de tarefas prontas

* Refatorar `AccountService` para reduzir alocações e melhorar legibilidade do hash/salt.
* Adicionar logs e mensagens de erro no `SimpleAuthenticator` quando o login falhar.
* Criar testes de unidade para `CharacterService.SavePosition/LoadPosition` (mockando dados).
* Introduzir `Result<T>` simples para operações de DB com códigos de erro padronizados (sem exceções como fluxo).

## Comunicação

* Prefixos de commit: `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`.
* PRs devem referenciar problemas/objetivos quando existirem (ex.: "fixes #12").

## Limites

* Não publicar segredos/keys; não mover binários grandes para fora do LFS.
* Não alterar `.unity`/`.prefab` sem instrução explícita.

---

### Apêndice A — Boilerplate de teste (sugestão)

* Criar pasta `Assets/Tests/Editor/` com testes `NUnit` para lógicas puras (ex.: hash).
* Evitar referências a `UnityEngine` quando possível.

### Apêndice B — Qualidade e ferramentas (opcional)

* `.editorconfig` básico para C# (indentação 4, `var` quando óbvio).
* Ativar **analisadores Roslyn** se desejado; manter nível de warning sob controle.

### Apêndice C — Comandos úteis (dev)

```
git status
git add .
git commit -m "refactor: melhorar hash e validação"
git push
```
