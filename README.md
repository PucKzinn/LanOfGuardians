# LanOfGuardians


## Teste manual (2 clientes)

1. Adicione **PlayerMovement2D** ao objeto do jogador contendo `Rigidbody2D` (gravity scale = 0) e mantenha `NetworkTransformHybrid` no objeto raiz.
2. Anexe **CameraFollow2D** à câmera principal.
3. Abra duas instâncias do jogo (ex.: Editor como Host e Build como Client).
4. No Host, inicie como servidor/jogador; no segundo cliente, conecte em `localhost`.
5. Use **WASD** para mover o jogador e verifique que o movimento é aplicado pelo servidor e sincronizado entre as instâncias.

## Troubleshooting de Login

- **Credenciais inválidas**: confirme usuário e senha; respeite maiúsculas/minúsculas.
- **Sem resposta do servidor**: verifique o endereço/IP e se o servidor está em execução.
- **Desconectado imediatamente**: cheque os logs do servidor para mensagens detalhadas.

Protótipo em Unity 2021.3 usando Mirror para rede e SQLite para persistência.

## Troubleshooting de Login
- **Servidor inatingível**: verifique IP/porta e se o servidor está em execução.
- **"Credenciais inválidas"**: confirme usuário e senha. Use a opção "criar conta" se necessário.
- **Authenticator não atribuído**: arraste `SimpleAuthenticator` para o `NetworkManager`.
- **Desconexão imediata**: confira se o banco de dados pode ser acessado (arquivo bloqueado ou ausente).


