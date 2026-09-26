using System.Buffers.Binary;
using System.Drawing.Drawing2D;
using Pes2019MlEditor.Core.Crypto;
using Pes2019MlEditor.Core.Finance;
using Pes2019MlEditor.Core.IO;
using Pes2019MlEditor.Core.Models;

namespace Pes2019MlEditor.UI;

public partial class MainForm : Form
{
    private PesSaveDescriptor? _currentSave;
    private string? _currentFilePath;
    private FinanceCandidate? _selectedCandidate;
    private string? _detectedSaveDirectory;

    // Controls
    private ComboBox _cboAutoSaves = null!;
    private Button _btnLoadAutoSave = null!;
    private Button _btnRefreshAutoSaves = null!;
    private TextBox _txtManualFilePath = null!;
    private Button _btnBrowseManual = null!;
    private Button _btnLoadManual = null!;
    private Label _lblStatus = null!;
    private Label _lblFilePath = null!;
    private PictureBox _picSaveLogo = null!;
    private Button _btnSaveFile = null!;
    private Button _btnBackup = null!;

    // Finance Controls
    private TextBox _txtTransferCurrent = null!;
    private TextBox _txtSalaryCurrent = null!;
    private Button _btnSearchBudget = null!;
    private ComboBox _cboCandidates = null!;
    private NumericUpDown _numNewTransfer = null!;
    private NumericUpDown _numNewSalary = null!;
    private Button _btnApplyBudget = null!;
    private TextBox _txtLog = null!;

    public MainForm()
    {
        InitializeComponents();
        ApplyDarkTheme();
        AutoDetectFolderAndPopulateSaves();
    }

    private void InitializeComponents()
    {
        Text = "PES 2019 Master League Editor v0.1 - [Finanças & Orçamento]";
        Size = new Size(880, 750);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(840, 700);
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12),
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 185)); // Header & File Selection (Auto + Manual)
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200)); // Finance Finder
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // Budget Editor
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Logs

        // 1. Header Group
        var grpHeader = new GroupBox
        {
            Text = "Carregar Save da Master Liga (Automático ou Manual)",
            Dock = DockStyle.Fill,
            ForeColor = Color.White
        };

        // --- Linha 1: Modo Automático ---
        var lblAutoTitle = new Label
        {
            Text = "Modo Automático (Saves detectados na pasta do PES):",
            Location = new Point(15, 25),
            AutoSize = true,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 210, 255)
        };

        _cboAutoSaves = new ComboBox
        {
            Location = new Point(15, 48),
            Size = new Size(330, 28),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _btnLoadAutoSave = new Button
        {
            Text = "⚡ Carregar Selecionado",
            Size = new Size(160, 28),
            Location = new Point(355, 47),
            Cursor = Cursors.Hand
        };
        _btnLoadAutoSave.Click += OnLoadAutoSaveClicked;

        _btnRefreshAutoSaves = new Button
        {
            Text = "🔄 Atualizar",
            Size = new Size(85, 28),
            Location = new Point(520, 47),
            Cursor = Cursors.Hand
        };
        _btnRefreshAutoSaves.Click += (s, e) => AutoDetectFolderAndPopulateSaves();

        // --- Linha 2: Modo Manual ---
        var lblManualTitle = new Label
        {
            Text = "Modo Manual (Selecione qualquer arquivo ou pasta):",
            Location = new Point(15, 82),
            AutoSize = true,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 210, 255)
        };

        _txtManualFilePath = new TextBox
        {
            Location = new Point(15, 105),
            Size = new Size(420, 26),
            PlaceholderText = "Cole o caminho ou clique em Procurar Save..."
        };

        _btnBrowseManual = new Button
        {
            Text = "📁 Procurar Save...",
            Size = new Size(130, 28),
            Location = new Point(445, 104),
            Cursor = Cursors.Hand
        };
        _btnBrowseManual.Click += OnBrowseManualClicked;

        _btnLoadManual = new Button
        {
            Text = "Abrir Caminho",
            Size = new Size(110, 28),
            Location = new Point(580, 104),
            Cursor = Cursors.Hand
        };
        _btnLoadManual.Click += OnLoadManualPathClicked;

        // --- Linha 3: Barra de Ações e Status ---
        _btnSaveFile = new Button
        {
            Text = "💾 Salvar Alterações",
            Size = new Size(150, 32),
            Location = new Point(15, 140),
            Enabled = false,
            Cursor = Cursors.Hand,
            BackColor = Color.FromArgb(30, 90, 60),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        _btnSaveFile.Click += OnSaveFileClicked;

        _btnBackup = new Button
        {
            Text = "🛡 Fazer Backup Manual",
            Size = new Size(160, 32),
            Location = new Point(175, 140),
            Enabled = false,
            Cursor = Cursors.Hand
        };
        _btnBackup.Click += OnManualBackupClicked;

        _lblFilePath = new Label
        {
            Text = "Nenhum save ativo.",
            Location = new Point(345, 147),
            AutoSize = true,
            ForeColor = Color.LightGray
        };

        _lblStatus = new Label
        {
            Text = "Pronto.",
            Location = new Point(550, 147),
            AutoSize = true,
            ForeColor = Color.Cyan
        };

        _picSaveLogo = new PictureBox
        {
            Location = new Point(745, 20),
            Size = new Size(95, 95),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle
        };

        grpHeader.Controls.AddRange([
            lblAutoTitle, _cboAutoSaves, _btnLoadAutoSave, _btnRefreshAutoSaves,
            lblManualTitle, _txtManualFilePath, _btnBrowseManual, _btnLoadManual,
            _btnSaveFile, _btnBackup, _lblFilePath, _lblStatus, _picSaveLogo
        ]);
        mainPanel.Controls.Add(grpHeader, 0, 0);

        // 2. Finance Finder Group
        var grpSearch = new GroupBox
        {
            Text = "1. Localizador de Finanças do Clube",
            Dock = DockStyle.Fill,
            ForeColor = Color.White
        };

        var lblHelp = new Label
        {
            Text = "Abra seu PES 2019, veja os valores em Sala do Treinador > Finanças > Saldo do Clube e digite abaixo:",
            Location = new Point(15, 25),
            AutoSize = true,
            ForeColor = Color.FromArgb(200, 200, 200)
        };

        var lblCurTransfer = new Label
        {
            Text = "Orçamento de Transferência atual (€):",
            Location = new Point(15, 52),
            AutoSize = true
        };
        _txtTransferCurrent = new TextBox
        {
            Location = new Point(15, 75),
            Size = new Size(200, 26),
            PlaceholderText = "Ex: 25000000"
        };

        var lblCurSalary = new Label
        {
            Text = "Orçamento de Salários atual (€) [Opcional mas recomendado]:",
            Location = new Point(235, 52),
            AutoSize = true
        };
        _txtSalaryCurrent = new TextBox
        {
            Location = new Point(235, 75),
            Size = new Size(220, 26),
            PlaceholderText = "Ex: 4800000"
        };

        _btnSearchBudget = new Button
        {
            Text = "🔍 Localizar Finanças no Save",
            Size = new Size(220, 32),
            Location = new Point(475, 72),
            Cursor = Cursors.Hand
        };
        _btnSearchBudget.Click += OnSearchBudgetClicked;

        var lblCandidate = new Label
        {
            Text = "Candidatos encontrados no save:",
            Location = new Point(15, 112),
            AutoSize = true
        };

        _cboCandidates = new ComboBox
        {
            Location = new Point(15, 135),
            Size = new Size(720, 28),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cboCandidates.SelectedIndexChanged += OnCandidateSelected;

        grpSearch.Controls.AddRange([lblHelp, lblCurTransfer, _txtTransferCurrent, lblCurSalary, _txtSalaryCurrent, _btnSearchBudget, lblCandidate, _cboCandidates]);
        mainPanel.Controls.Add(grpSearch, 0, 1);

        // 3. Budget Editor Group
        var grpEdit = new GroupBox
        {
            Text = "2. Editar Valores de Orçamento",
            Dock = DockStyle.Fill,
            ForeColor = Color.White
        };

        var lblNewTrans = new Label
        {
            Text = "Novo Orçamento de Transferências (€):",
            Location = new Point(15, 25),
            AutoSize = true
        };
        _numNewTransfer = new NumericUpDown
        {
            Location = new Point(15, 50),
            Size = new Size(220, 26),
            Maximum = 1_999_999_999,
            ThousandsSeparator = true
        };

        var lblNewSal = new Label
        {
            Text = "Novo Orçamento de Salários (€):",
            Location = new Point(255, 25),
            AutoSize = true
        };
        _numNewSalary = new NumericUpDown
        {
            Location = new Point(255, 50),
            Size = new Size(220, 26),
            Maximum = 1_999_999_999,
            ThousandsSeparator = true
        };

        _btnApplyBudget = new Button
        {
            Text = "✔ Aplicar Alterações no Save",
            Size = new Size(210, 32),
            Location = new Point(495, 47),
            Cursor = Cursors.Hand,
            Enabled = false
        };
        _btnApplyBudget.Click += OnApplyBudgetClicked;

        // Quick presets
        var btnAdd50M = new Button { Text = "+ €50M Transf", Size = new Size(110, 28), Location = new Point(15, 95) };
        btnAdd50M.Click += (s, e) => _numNewTransfer.Value = Math.Min(_numNewTransfer.Maximum, _numNewTransfer.Value + 50_000_000);

        var btnAdd100M = new Button { Text = "+ €100M Transf", Size = new Size(110, 28), Location = new Point(130, 95) };
        btnAdd100M.Click += (s, e) => _numNewTransfer.Value = Math.Min(_numNewTransfer.Maximum, _numNewTransfer.Value + 100_000_000);

        var btnAdd10MSal = new Button { Text = "+ €10M Salário", Size = new Size(110, 28), Location = new Point(255, 95) };
        btnAdd10MSal.Click += (s, e) => _numNewSalary.Value = Math.Min(_numNewSalary.Maximum, _numNewSalary.Value + 10_000_000);

        var btnAdd25MSal = new Button { Text = "+ €25M Salário", Size = new Size(110, 28), Location = new Point(370, 95) };
        btnAdd25MSal.Click += (s, e) => _numNewSalary.Value = Math.Min(_numNewSalary.Maximum, _numNewSalary.Value + 25_000_000);

        var btnMaxAll = new Button { Text = "⭐ Max Finanças (€500M / €100M)", Size = new Size(220, 28), Location = new Point(495, 95) };
        btnMaxAll.Click += (s, e) =>
        {
            _numNewTransfer.Value = 500_000_000;
            _numNewSalary.Value = 100_000_000;
        };

        grpEdit.Controls.AddRange([lblNewTrans, _numNewTransfer, lblNewSal, _numNewSalary, _btnApplyBudget, btnAdd50M, btnAdd100M, btnAdd10MSal, btnAdd25MSal, btnMaxAll]);
        mainPanel.Controls.Add(grpEdit, 0, 2);

        // 4. Console / Log
        var grpLog = new GroupBox
        {
            Text = "Registro de Atividades (Logs)",
            Dock = DockStyle.Fill,
            ForeColor = Color.White
        };
        _txtLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9f)
        };
        grpLog.Controls.Add(_txtLog);
        mainPanel.Controls.Add(grpLog, 0, 3);

        Controls.Add(mainPanel);
    }

    private void ApplyDarkTheme()
    {
        BackColor = Color.FromArgb(24, 24, 37);
        ForeColor = Color.FromArgb(220, 220, 230);

        ApplyDarkThemeRecursive(this);
    }

    private void ApplyDarkThemeRecursive(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            if (c is Button btn)
            {
                btn.BackColor = Color.FromArgb(45, 52, 71);
                btn.ForeColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.FromArgb(70, 80, 110);
            }
            else if (c is TextBox or NumericUpDown or ComboBox)
            {
                c.BackColor = Color.FromArgb(35, 38, 55);
                c.ForeColor = Color.White;
            }
            else if (c is GroupBox grp)
            {
                grp.ForeColor = Color.FromArgb(100, 200, 255);
            }

            if (c.HasChildren)
            {
                ApplyDarkThemeRecursive(c);
            }
        }
    }

    private void AutoDetectFolderAndPopulateSaves()
    {
        _cboAutoSaves.Items.Clear();
        _detectedSaveDirectory = SaveFileManager.TryDetectSaveDirectory();

        if (_detectedSaveDirectory != null)
        {
            Log($"Pasta padrão do PES 2019 detectada: {_detectedSaveDirectory}");
            var saves = SaveFileManager.GetMasterLeagueSaves(_detectedSaveDirectory);

            if (saves.Length > 0)
            {
                foreach (var savePath in saves)
                {
                    _cboAutoSaves.Items.Add(new SaveItem(savePath));
                }
                _cboAutoSaves.SelectedIndex = 0;
                _btnLoadAutoSave.Enabled = true;
                Log($"Saves de Master Liga encontrados automaticamente: {saves.Length}");
            }
            else
            {
                _cboAutoSaves.Items.Add("Nenhum arquivo ML* encontrado na pasta padrão");
                _cboAutoSaves.SelectedIndex = 0;
                _btnLoadAutoSave.Enabled = false;
                Log("Nenhum arquivo ML* encontrado na pasta padrão.");
            }
        }
        else
        {
            _cboAutoSaves.Items.Add("Pasta KONAMI não encontrada automaticamente");
            _cboAutoSaves.SelectedIndex = 0;
            _btnLoadAutoSave.Enabled = false;
            Log("Pasta KONAMI/PRO EVOLUTION SOCCER 2019 não localizada em Documentos. Utilize a busca manual.");
        }
    }

    private void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        _txtLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
    }

    private void LoadSaveFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            MessageBox.Show($"O arquivo especificado não existe:\n{filePath}", "Arquivo não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _currentFilePath = filePath;
            _txtManualFilePath.Text = filePath;
            _currentSave = SaveFileManager.LoadSave(_currentFilePath);

            _lblFilePath.Text = Path.GetFileName(_currentFilePath);
            _lblStatus.Text = $"Save Carregado! (Payload: {_currentSave.Data.Length:N0} bytes)";
            _btnSaveFile.Enabled = true;
            _btnBackup.Enabled = true;

            Log($"Save carregado com sucesso: {_currentFilePath}");
            Log($"Tipo: {_currentSave.FileHeader.FileTypeString} | Versão: {_currentSave.FileHeader.GameVersionString} | Tamanho Payload: {_currentSave.Data.Length:N0} bytes");

            // Render logo if available
            if (_currentSave.Logo.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(_currentSave.Logo);
                    _picSaveLogo.Image = Image.FromStream(ms);
                }
                catch
                {
                    _picSaveLogo.Image = null;
                }
            }
            else
            {
                _picSaveLogo.Image = null;
            }

            _cboCandidates.Items.Clear();

            // Auto-detect and pre-load club finances if found at default offset
            if (_currentSave.Data.Length > MlFinanceService.DefaultBalanceOffset + 4)
            {
                int currentTransRaw = BinaryPrimitives.ReadInt32LittleEndian(_currentSave.Data.AsSpan(MlFinanceService.DefaultTransferOffset, 4));
                int currentBalRaw = BinaryPrimitives.ReadInt32LittleEndian(_currentSave.Data.AsSpan(MlFinanceService.DefaultBalanceOffset, 4));

                if (currentTransRaw > 0)
                {
                    long transEur = (long)currentTransRaw * 100;
                    long balEur = (long)currentBalRaw * 100;

                    _txtTransferCurrent.Text = transEur.ToString();
                    _txtSalaryCurrent.Text = balEur.ToString();

                    var candidate = new FinanceCandidate
                    {
                        Offset = MlFinanceService.DefaultTransferOffset,
                        FoundValue = transEur,
                        SecondValueOffset = MlFinanceService.DefaultBalanceOffset,
                        SecondValue = balEur,
                        ProximityToSecondValue = 16
                    };

                    _cboCandidates.Items.Add(candidate);
                    _cboCandidates.SelectedIndex = 0;
                    _btnApplyBudget.Enabled = true;

                    Log($"Finanças do clube detectadas automaticamente: Transferência = {transEur:N0} €, Saldo da Conta = {balEur:N0} €");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao descriptografar save: {ex.Message}", "Erro de Leitura", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log($"Falha ao abrir save: {ex.Message}");
        }
    }

    private void OnLoadAutoSaveClicked(object? sender, EventArgs e)
    {
        if (_cboAutoSaves.SelectedItem is SaveItem item)
        {
            LoadSaveFile(item.FullPath);
        }
    }

    private void OnBrowseManualClicked(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Selecione manualmente o arquivo de save da Master Liga",
            Filter = "Saves da Master Liga (ML*)|ML*|Todos os Arquivos (*.*)|*.*",
            InitialDirectory = _detectedSaveDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            LoadSaveFile(ofd.FileName);
        }
    }

    private void OnLoadManualPathClicked(object? sender, EventArgs e)
    {
        string path = _txtManualFilePath.Text.Trim('"', ' ', '\t');
        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show("Cole ou digite o caminho completo do arquivo de save.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        LoadSaveFile(path);
    }

    private void OnSearchBudgetClicked(object? sender, EventArgs e)
    {
        if (_currentSave == null)
        {
            MessageBox.Show("Abra um arquivo de save antes de buscar os valores.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!long.TryParse(_txtTransferCurrent.Text.Trim().Replace(".", "").Replace(",", ""), out long transVal))
        {
            MessageBox.Show("Informe o valor atual do Orçamento de Transferências exibido no PES.", "Valor Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        long? salVal = null;
        if (long.TryParse(_txtSalaryCurrent.Text.Trim().Replace(".", "").Replace(",", ""), out long parsedSal))
        {
            salVal = parsedSal;
        }

        Log($"Pesquisando no save por Transferência = {transVal:N0} €" + (salVal.HasValue ? $" e Salários = {salVal:N0} €..." : "..."));

        var candidates = MlFinanceService.FindCandidates(_currentSave.Data, transVal, salVal);
        _cboCandidates.Items.Clear();

        if (candidates.Count == 0)
        {
            Log("Nenhuma ocorrência encontrada com esse valor exato.");
            MessageBox.Show("Nenhum bloco financeiro encontrado com esse valor exato.\nCertifique-se de que o jogo está em Euros ou de digitar o número sem pontuação.", "Não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (var c in candidates)
        {
            _cboCandidates.Items.Add(c);
        }

        _cboCandidates.SelectedIndex = 0;
        Log($"{candidates.Count} candidato(s) localizado(s) no buffer.");
    }

    private void OnCandidateSelected(object? sender, EventArgs e)
    {
        if (_cboCandidates.SelectedItem is FinanceCandidate candidate && _currentSave != null)
        {
            _selectedCandidate = candidate;
            _numNewTransfer.Value = candidate.FoundValue;

            if (candidate.SecondValueOffset >= 0)
            {
                _numNewSalary.Value = candidate.SecondValue;
            }
            else
            {
                // Inspect nearby 8 bytes forward/backward
                try
                {
                    int nearby = MlFinanceService.ReadInt32(_currentSave.Data, candidate.Offset + 8);
                    _numNewSalary.Value = Math.Max(0, nearby);
                }
                catch
                {
                    _numNewSalary.Value = 0;
                }
            }

            _btnApplyBudget.Enabled = true;
            Log($"Candidato selecionado: Offset 0x{candidate.Offset:X8}.");
        }
    }

    private void OnApplyBudgetClicked(object? sender, EventArgs e)
    {
        if (_currentSave == null || _selectedCandidate == null)
        {
            return;
        }

        long newTrans = (long)_numNewTransfer.Value;
        long newSal = (long)_numNewSalary.Value;

        // Update transfer budget (scaled / 100 for PES 2019)
        MlFinanceService.WriteScaledBudget(_currentSave.Data, _selectedCandidate.Offset, newTrans);

        // Update salary/balance budget if identified
        if (_selectedCandidate.SecondValueOffset >= 0)
        {
            MlFinanceService.WriteScaledBudget(_currentSave.Data, _selectedCandidate.SecondValueOffset, newSal);
        }

        Log($"Alterações aplicadas na memória: Transferência = {newTrans:N0} €, Saldo/Teto = {newSal:N0} €.");
        MessageBox.Show("Valores atualizados na memória com sucesso!\nClique em 'Salvar Alterações' para gravar o arquivo ML.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnSaveFileClicked(object? sender, EventArgs e)
    {
        if (_currentSave == null || string.IsNullOrEmpty(_currentFilePath))
        {
            return;
        }

        try
        {
            string savedPath = SaveFileManager.SaveWithBackup(_currentFilePath, _currentSave, createBackup: true);
            Log($"Save re-criptografado e salvo com sucesso em: {savedPath}");
            Log($"Backup de segurança criado automaticamente (.bak e .bak_timestamp).");
            MessageBox.Show($"Save salvo com sucesso!\n\nUm backup de segurança foi criado antes de salvar.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar arquivo: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log($"Erro ao salvar: {ex.Message}");
        }
    }

    private void OnManualBackupClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
        {
            MessageBox.Show("Nenhum save carregado para fazer backup.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            string dir = Path.GetDirectoryName(_currentFilePath)!;
            string fileName = Path.GetFileName(_currentFilePath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(dir, $"{fileName}.manual_bak_{timestamp}");
            File.Copy(_currentFilePath, backupPath, overwrite: true);
            Log($"Backup manual criado: {backupPath}");
            MessageBox.Show($"Backup manual criado:\n{backupPath}", "Backup Criado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao criar backup manual: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class SaveItem(string fullPath)
    {
        public string FullPath { get; } = fullPath;

        public override string ToString()
        {
            var info = new FileInfo(FullPath);
            return $"{info.Name}  ({info.Length / 1024:N0} KB - {info.LastWriteTime:dd/MM/yyyy HH:mm})";
        }
    }
}
