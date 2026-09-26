# PES Master League Editor Ultra ⚽💰

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20x64-blue?logo=windows&logoColor=white)](https://github.com/SouzaDH91/PES-Master-League-Editor-Ultra/releases/latest)
[![Release](https://img.shields.io/badge/release/SouzaDH91/PES-Master-League-Editor-Ultra?style=for-the-badge)](https://github.com/SouzaDH91/PES-Master-League-Editor-Ultra/releases/latest)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Editor moderno de arquivos de save da Master Liga (`ML00000000`) para o **Pro Evolution Soccer (PC)** desenvolvido em **C# / .NET**.

---

## ✨ Funcionalidades Principais (v0.1.0)

- 🔓 **Motor Criptográfico Nativo Konami (C#)**:
  - Implementação integral do algoritmo proprietário da Konami baseado no PRNG **Mersenne Twister (MT19937)** com a Master Key original do PES 2019.
  - Suporte ao header de criptografia de 320 bytes (`EncryptionHeader`), metadados de cabeçalho (`FileHeader`), miniatura PNG do save e payload (`data.dat`).
  - Criptografia simétrica com recálculo automático de integridade e checagens de payload, garantindo 100% de compatibilidade sem o jogo acusar "dados corrompidos".
- 💰 **Edição Completa de Finanças & Orçamentos**:
  - Ajuste direto do **Orçamento de Transferências**.
  - Ajuste do **Caixa Total do Clube** (que define e expande o teto do **Orçamento de Salários**).
  - Atalhos práticos de valor: `+ €50M`, `+ €100M`, `+ €10M Salário`, `Max Finanças`.
- 🛡️ **Sistema de Backup Automático & Manual**:
  - Criação automática de cópias de segurança a cada salvamento:
    - `ML00000000.bak` (última versão antes da alteração)
    - `ML00000000.bak_YYYYMMDD_HHMMSS` (histórico com data e hora)
- 🎨 **Interface Gráfica Moderna (Dark Theme)**:
  - Detecção automática de diretórios de save do PES 2019 na pasta Documentos (Steam e patches como HANO4U).
  - Visualização da logo/miniatura original do save.
  - Console de log em tempo real integrado na tela.
  - Executável independente (**Single-File standalone**), não necessita instalar o .NET no computador do usuário.

---

## 📥 Download & Instalação

1. Vá até a aba de **[Releases](https://github.com/)** e baixe o arquivo `Pes2019MlEditor-v0.1.0-win-x64.zip`.
2. Extraia o arquivo `.zip` em qualquer pasta do seu computador.
3. Execute o `Pes2019MlEditor.UI.exe`.

---

## 🎮 Como Usar

1. **Abra o programa**. Ele tentará detectar automaticamente seus arquivos de save da Master Liga.
2. Selecione o save desejado no menu suspenso ou clique em **Procurar...** para selecionar o arquivo manualmente (`ML00000000`, `ML00000001`, etc.).
3. O editor carregará a miniatura do save e detectará os valores atuais de orçamento.
4. Ajuste os valores desejados nos campos de **Orçamento de Transferência** e **Caixa Total / Salários** (ou use os botões de atalho rápido).
5. Clique em **Salvar Alterações**. O editor salvará o arquivo recriptografado e gerará um backup `.bak`.
6. Abra o **PES 2019** e carregue o seu save normalmente!

---

## 🏗 Arquitetura da Solução

```text
PES 2019 Master League Editor/
├── src/
│   ├── Pes2019MlEditor.Core/          # Lógica central: Criptografia, IO e Serviços Financeiros
│   │   ├── Crypto/
│   │   │   ├── MersenneTwister.cs     # Gerador de números pseudo-aleatórios MT19937 (32-bit)
│   │   │   └── Pes19Crypto.cs         # Descriptografia e Re-criptografia do envelope Konami
│   │   ├── Finance/
│   │   │   ├── FinanceCandidate.cs    # Modelo de registro financeiro
│   │   │   └── MlFinanceService.cs    # Mapeamento e edição de orçamentos (escala /100)
│   │   ├── IO/
│   │   │   └── SaveFileManager.cs     # Gerenciamento de arquivos e backups automáticos
│   │   └── Models/
│   │       ├── FileHeader.cs          # Metadados de cabeçalho do save (208 bytes)
│   │       └── PesSaveDescriptor.cs   # Representação em memória do save decodificado
│   └── Pes2019MlEditor.UI/            # Interface gráfica Windows Forms moderna
│       ├── MainForm.cs                # Formulário principal e interações
│       └── Program.cs                 # Ponto de entrada
└── tests/
    └── Pes2019MlEditor.Tests/         # Suíte de testes unitários (xUnit)
        ├── CryptoTests.cs             # Testes de integridade criptográfica
        └── FinanceTests.cs            # Testes de busca e edição de dados financeiros
```

---

## 🗺 Roadmap de Versões

- [x] **v0.1**: Motor criptográfico nativo + Edição de Orçamento de Transferências e Salários + Backups automáticos.
- [ ] **v0.2**: Edição de Calendário (data da temporada, avanço de semanas) e configurações da carreira.
- [ ] **v0.3**: Gestão do Elenco (contratos restantes, salários individuais de jogadores, moral e espírito de equipe).
- [ ] **v0.4**: Status e Atributos de Jogadores (overall, habilidades e fadiga).
- [ ] **v0.5**: Transferências Forçadas entre clubes diretamente no save.
- [ ] **v0.6**: Suporte Multi-Versões do PES (adaptação para PES 2016-2021 com chaves dedicadas e legado PES 2013-2015).

---

## ⚠️ Isenção de Responsabilidade

Este projeto é desenvolvido para fins educacionais e de modding pela comunidade. Pro Evolution Soccer é uma marca registrada da Konami Digital Entertainment. Faça sempre backup dos seus arquivos de save.
