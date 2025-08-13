# LanOfGuardians

Protótipo em Unity 2021.3 usando Mirror para rede e SQLite para persistência.

## Troubleshooting de Login
- **Servidor inatingível**: verifique IP/porta e se o servidor está em execução.
- **"Credenciais inválidas"**: confirme usuário e senha. Use a opção "criar conta" se necessário.
- **Authenticator não atribuído**: arraste `SimpleAuthenticator` para o `NetworkManager`.
- **Desconexão imediata**: confira se o banco de dados pode ser acessado (arquivo bloqueado ou ausente).
