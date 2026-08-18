# Contribuindo com Rapadura

## Branches
- `main` — sempre estável/buildável.
- `feature/<nome-curto>` — uma feature por branch.
- `fix/<nome-curto>` — correções de bug.
- Sem push direto na `main`: toda mudança entra via Pull Request.

## Commits
Mensagens curtas e no imperativo, descrevendo o "porquê" quando não for óbvio (ex: `fix: corrige stamina negativa ao correr sem fôlego`).

## Definition of Done (por feature)
Uma tarefa do `TODO.md` só é marcada `[x]` quando:
1. Código implementado e compilando sem warnings novos.
2. Testado manualmente no Editor (Play Mode) cobrindo o caminho feliz e pelo menos um caso de borda.
3. Teste automatizado adicionado quando a lógica for testável fora do MonoBehaviour (ex: StateMachine, cálculo de dano, save/load).
4. Revisão de código (PR) aprovada por outra pessoa (ou autorrevisão documentada, se solo).
5. `TODO.md` atualizado (item marcado + changelog, se for mudança de escopo).

## Padrões de Código
- Namespaces sempre `Rapadura.<Camada>.<Subsistema>` (ver `Assets/Scripts`).
- Campos privados serializados: `_camelCase` (ver `.editorconfig`).
- Managers implementam `IManager`; dados persistíveis implementam `ISaveable`.
- Preferir `ServiceLocator`/injeção de dependência a singletons diretos (`GameManager` é a exceção documentada).
- Sem comentários explicando o óbvio — só o porquê quando não for evidente pelo código.

## Testes
Testes automatizados vivem em `Assets/Tests` (EditMode) separados por assembly definition da pasta `Scripts`, para não inflar o build final.

## Antes de abrir PR
- [ ] Rodar os testes (Window → General → Test Runner no Unity Editor)
- [ ] Conferir que não há assets/cenas quebrados (ícone de erro no Console)
- [ ] Atualizar `TODO.md` se o escopo mudou
