# LanOfGuardians

## Teste manual (2 clientes)

1. Adicione **PlayerMovement2D** ao objeto do jogador contendo `Rigidbody2D` (gravity scale = 0) e mantenha `NetworkTransformHybrid` no objeto raiz.
2. Anexe **CameraFollow2D** à câmera principal.
3. Abra duas instâncias do jogo (ex.: Editor como Host e Build como Client).
4. No Host, inicie como servidor/jogador; no segundo cliente, conecte em `localhost`.
5. Use **WASD** para mover o jogador e verifique que o movimento é aplicado pelo servidor e sincronizado entre as instâncias.
