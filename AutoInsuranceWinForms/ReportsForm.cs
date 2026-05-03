using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
        public class ReportsForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly ComboBox _cb = Theme.CreateComboBox(300);
        private static readonly Encoding ExportEncoding = new UTF8Encoding(true);

        public ReportsForm()
        {
            Theme.StyleForm(this); Text = "Отчеты"; Width = 1200; Height = 700; StartPosition = FormStartPosition.CenterParent; Theme.StyleGrid(_grid);
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 92, Padding = new Padding(12), BackColor = Theme.Surface };
            _cb.Items.AddRange(new object[] { "Договоры по типам", "Выплаты по страховым случаям", "Активные договоры", "Автомобили по категориям" }); _cb.SelectedIndex = 0; _cb.SelectedIndexChanged += delegate { BuildReport(); };
            var export = Theme.CreateSecondaryButton("Экспорт", 130); export.Click += delegate { Export(); };
            top.Controls.Add(new Label { Text = "Тип отчета:", AutoSize = true, Padding = new Padding(0, 9, 0, 0) }); top.Controls.Add(_cb); top.Controls.Add(export);
            Controls.Add(_grid); Controls.Add(top); Load += delegate { BuildReport(); };
        }

        private void BuildReport()
        {
            switch (_cb.SelectedIndex)
            {
                case 0:
                    _grid.DataSource = Db.Query(@"SELECT t.type_name AS [Тип страхования], COUNT(*) AS [Количество договоров] FROM Contract c INNER JOIN insurance_types t ON t.id_type=c.id_type GROUP BY t.type_name ORDER BY [Количество договоров] DESC");
                    break;
                case 1:
                    _grid.DataSource = Db.Query(@"SELECT ic.case_id AS [Страховой случай], ic.brief_description AS [Описание], ic.final_damage AS [Ущерб], ISNULL(SUM(p.payout_amount),0) AS [Выплачено] FROM Insurance_cases ic LEFT JOIN Insurance_payouts p ON p.case_id=ic.case_id GROUP BY ic.case_id, ic.brief_description, ic.final_damage ORDER BY ic.case_id DESC");
                    break;
                case 2:
                    _grid.DataSource = Db.Query(@"SELECT c.id_contract AS [Договор], t.type_name AS [Тип], c.start_date AS [Начало], c.end_date AS [Окончание], c.insurance_amount AS [Сумма], c.VIN AS [VIN] FROM Contract c INNER JOIN insurance_types t ON t.id_type=c.id_type WHERE c.end_date >= CAST(GETDATE() AS DATE) ORDER BY c.end_date");
                    break;
                default:
                    _grid.DataSource = Db.Query(@"SELECT vc.category_name AS [Категория], COUNT(*) AS [Количество автомобилей] FROM Vehicles v INNER JOIN vehicle_categories vc ON vc.id_vehicle_category=v.id_vehicle_category GROUP BY vc.category_name ORDER BY [Количество автомобилей] DESC");
                    break;
            }
        }

        private void Export()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV (*.csv)|*.csv|Text (*.txt)|*.txt|JSON (*.json)|*.json|Excel (*.xlsx)|*.xlsx|TSV (*.tsv)|*.tsv|XML (*.xml)|*.xml";
                sfd.FileName = "report.csv";
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                var ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                if (ext == ".csv") ExportDelimited(sfd.FileName, ";");
                else if (ext == ".txt") ExportDelimited(sfd.FileName, " | ");
                else if (ext == ".tsv" || ext == ".xlsx") ExportDelimited(sfd.FileName, "\t");
                else if (ext == ".json") ExportJson(sfd.FileName);
                else if (ext == ".xml") ExportXml(sfd.FileName);
                else ExportDelimited(sfd.FileName, ";");
                MessageBox.Show("Экспорт завершен.");
            }
        }

        private void ExportDelimited(string fileName, string separator)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(separator, _grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText)));
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                sb.AppendLine(string.Join(separator, row.Cells.Cast<DataGridViewCell>().Select(c => GetCellText(c).Replace(separator, " "))));
            }
            File.WriteAllText(fileName, sb.ToString(), ExportEncoding);
        }

        private void ExportJson(string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            var rows = _grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToList();
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                sb.Append("  {");
                for (var j = 0; j < _grid.Columns.Count; j++)
                {
                    var col = _grid.Columns[j];
                    var key = EscapeJson(col.HeaderText);
                    var value = EscapeJson(GetCellText(row.Cells[j]));
                    sb.Append("\"" + key + "\":\"" + value + "\"");
                    if (j < _grid.Columns.Count - 1) sb.Append(", ");
                }
                sb.Append("}");
                if (i < rows.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("]");
            File.WriteAllText(fileName, sb.ToString(), ExportEncoding);
        }

        private void ExportXml(string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<report>");
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                sb.AppendLine("  <row>");
                for (var i = 0; i < _grid.Columns.Count; i++)
                {
                    var col = _grid.Columns[i];
                    var name = SafeXmlName(col.HeaderText);
                    var value = SecurityElement.Escape(GetCellText(row.Cells[i])) ?? string.Empty;
                    sb.AppendLine("    <" + name + ">" + value + "</" + name + ">");
                }
                sb.AppendLine("  </row>");
            }
            sb.AppendLine("</report>");
            File.WriteAllText(fileName, sb.ToString(), ExportEncoding);
        }

        private string GetCellText(DataGridViewCell cell)
        {
            return (cell.Value ?? string.Empty).ToString() ?? string.Empty;
        }

        private string EscapeJson(string text)
        {
            return text.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private string SafeXmlName(string text)
        {
            var cleaned = new string(text.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
            if (cleaned.Length == 0) return "field";
            if (char.IsDigit(cleaned[0])) cleaned = "f_" + cleaned;
            return cleaned;
        }
    }
}
