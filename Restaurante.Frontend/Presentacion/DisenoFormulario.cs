namespace Restaurante.Frontend.Presentacion;
internal static class DisenoFormulario
{
    public static void EnInterfaz(Form form, Action accion)
    {
        if (form.IsDisposed || !form.IsHandleCreated)
            return;
        if (!form.InvokeRequired)
        {
            accion();
            return;
        }

        try
        {
            form.BeginInvoke((Action)(() =>
            {
                if (!form.IsDisposed)
                    accion();
            }));
        }
        catch (InvalidOperationException)when (form.IsDisposed || !form.IsHandleCreated)
        { /* La ventana ya terminó; no se actualizan controles destruidos. */
        }
    }

    public static FlowLayoutPanel Preparar(Form form, string titulo)
    {
        form.Text = titulo;
        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.AutoScaleDimensions = new SizeF(96, 96);
        form.Font = new Font("Segoe UI", 10);
        form.Size = new Size(760, 780);
        form.MinimumSize = new Size(560, 520);
        form.StartPosition = FormStartPosition.CenterParent;
        form.BackColor = Color.FromArgb(248, 248, 245);
        form.Shown += (_, _) =>
        {
            var area = Screen.FromControl(form).WorkingArea;
            form.MinimumSize = new Size(Math.Min(form.MinimumSize.Width, area.Width), Math.Min(form.MinimumSize.Height, area.Height));
            form.Size = new Size(Math.Min(form.Width, area.Width), Math.Min(form.Height, area.Height));
            form.Location = new Point(Math.Clamp(form.Left, area.Left, Math.Max(area.Left, area.Right - form.Width)), Math.Clamp(form.Top, area.Top, Math.Max(area.Top, area.Bottom - form.Height)));
        };
        var flpPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(20),
            Name = "flpPanel"
        };
        var flpPie = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            WrapContents = true,
            Padding = new Padding(16, 8, 16, 8),
            BackColor = Color.White,
            Name = "flpPie"
        };
        flpPanel.Tag = flpPie;
        form.Controls.Add(flpPanel);
        form.Controls.Add(flpPie);
        flpPanel.Controls.Add(new Label { Text = titulo, AutoSize = true, MaximumSize = new Size(650, 0), Font = new Font("Segoe UI", 16, FontStyle.Bold) });
        void AjustarAnchos()
        {
            var ancho = Math.Max(160, flpPanel.ClientSize.Width - flpPanel.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 8);
            foreach (Control c in flpPanel.Controls)
            {
                if (c is TextBox or ComboBox or DataGridView)
                    c.Width = ancho;
                if (c is Label)
                    c.MaximumSize = new Size(ancho, 0);
                if (c is CheckBox cb)
                {
                    cb.AutoSize = false;
                    cb.MaximumSize = Size.Empty;
                    cb.Width = ancho;
                    cb.Height = Math.Max(32, TextRenderer.MeasureText(cb.Text, cb.Font, new Size(Math.Max(100, ancho - 28), int.MaxValue), TextFormatFlags.WordBreak).Height + 12);
                }
            }

            var anchoPie = Math.Max(160, flpPie.ClientSize.Width - flpPie.Padding.Horizontal - 8);
            foreach (Control c in flpPie.Controls)
                if (c is Label)
                    c.MaximumSize = new Size(anchoPie, 0);
        }

        flpPanel.SizeChanged += (_, _) => AjustarAnchos();
        flpPanel.ControlAdded += (_, _) => AjustarAnchos();
        flpPie.SizeChanged += (_, _) => AjustarAnchos();
        form.Shown += (_, _) => AjustarAnchos();
        return flpPanel;
    }

    public static TextBox Campo(FlowLayoutPanel panel, string etiqueta, bool secreto = false)
    {
        panel.Controls.Add(new Label { Text = etiqueta, AutoSize = true, Margin = new Padding(0, 10, 0, 3) });
        var ctC = new TextBox
        {
            Width = 490,
            UseSystemPasswordChar = secreto,
            MaxLength = secreto ? 128 : 250,
            Name = "ctC"
        };
        panel.Controls.Add(ctC);
        return ctC;
    }

    public static ComboBox Opciones(FlowLayoutPanel panel, string etiqueta, params string[] opciones)
    {
        panel.Controls.Add(new Label { Text = etiqueta, AutoSize = true });
        var cbC = new ComboBox
        {
            Width = 490,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Name = "cbC"
        };
        cbC.Items.AddRange(opciones);
        if (cbC.Items.Count > 0)
            cbC.SelectedIndex = 0;
        panel.Controls.Add(cbC);
        return cbC;
    }

    public static Button Boton(FlowLayoutPanel p, string texto, EventHandler accion)
    {
        var btnB = new Button
        {
            Text = texto,
            AutoSize = true,
            MinimumSize = new Size(150, 40),
            Margin = new Padding(0, 4, 12, 4),
            BackColor = Color.FromArgb(49, 86, 62),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Name = "btnB"
        };
        btnB.Click += accion;
        (p.Tag as FlowLayoutPanel ?? p).Controls.Add(btnB);
        return btnB;
    }

    public static Label Mensaje(FlowLayoutPanel p)
    {
        var lblL = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(650, 0),
            ForeColor = Color.DarkRed,
            Name = "lblL"
        };
        var flpPie = p.Tag as FlowLayoutPanel ?? p;
        flpPie.SetFlowBreak(lblL, true);
        flpPie.Controls.Add(lblL);
        return lblL;
    }
}
