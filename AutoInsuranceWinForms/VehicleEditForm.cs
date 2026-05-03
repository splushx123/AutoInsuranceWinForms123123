using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class VehicleEditForm : Form
    {
        private readonly string _vinKey;
        private bool _isBindingVehicleFields;
        private readonly TextBox _vin = Theme.CreateTextBox(220);
        private readonly TextBox _plate = Theme.CreateTextBox(220);
        private readonly ComboBox _brand = Theme.CreateComboBox(220);
        private readonly ComboBox _model = Theme.CreateComboBox(220);
        private readonly ComboBox _category = Theme.CreateComboBox(220);
        private readonly NumericUpDown _power = Theme.CreateNumeric(220, 5000);
        private readonly TextBox _ptsSeries = Theme.CreateTextBox(220);
        private readonly TextBox _ptsNumber = Theme.CreateTextBox(220);
        private readonly ComboBox _client = Theme.CreateComboBox(220);

        public VehicleEditForm(string vin)
        {
            _vinKey = vin;
            Theme.StyleForm(this);
            Text = string.IsNullOrEmpty(vin) ? "Добавление автомобиля" : "Изменение автомобиля";
            Width = 650; Height = 520; StartPosition = FormStartPosition.CenterParent;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16), AutoScroll = true };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            AddField(table, "VIN", _vin); AddField(table, "Госномер", _plate); AddField(table, "Марка", _brand); AddField(table, "Модель", _model);
            AddField(table, "Категория ТС", _category); AddField(table, "Мощность", _power); AddField(table, "Серия ПТС", _ptsSeries); AddField(table, "Номер ПТС", _ptsNumber); AddField(table, "Владелец", _client);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
            var btnSave = Theme.CreatePrimaryButton("Сохранить", 120); btnSave.Click += delegate { SaveData(); };
            var btnCancel = Theme.CreateSecondaryButton("Отмена", 120); btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            buttons.Controls.Add(btnSave); buttons.Controls.Add(btnCancel);
            Controls.Add(table); Controls.Add(buttons);
            ConfigureInputRules();
            FillCombos(); if (!string.IsNullOrEmpty(vin)) LoadData();
        }

        private void ConfigureInputRules()
        {
            _vin.MaxLength = 17;
            _plate.MaxLength = 9;
            _ptsSeries.MaxLength = 4;
            _ptsNumber.MaxLength = 6;
            _vin.CharacterCasing = CharacterCasing.Upper;
            _plate.CharacterCasing = CharacterCasing.Upper;

            _vin.KeyPress += VinKeyPress;
            _ptsSeries.KeyPress += DigitsOnlyKeyPress;
            _ptsNumber.KeyPress += DigitsOnlyKeyPress;
            _brand.SelectedValueChanged += delegate { HandleBrandChanged(); };
            _model.SelectedValueChanged += delegate { HandleModelChanged(); };
        }

        private void FillCombos()
        {
            LookupService.Fill(_brand, "SELECT id_brand, brand_name FROM car_brands ORDER BY brand_name", "id_brand", "brand_name");
            LookupService.Fill(_category, "SELECT id_vehicle_category, category_name FROM vehicle_categories ORDER BY category_name", "id_vehicle_category", "category_name");
            LookupService.Fill(_client, "SELECT id_client, last_name + ' ' + first_name AS fio FROM Client ORDER BY last_name, first_name", "id_client", "fio");
            HandleBrandChanged();
        }

        private void AddField(TableLayoutPanel t, string name, Control control)
        {
            int r = t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            t.Controls.Add(new Label { Text = name, AutoSize = true, Padding = new Padding(0, 9, 0, 0) }, 0, r); t.Controls.Add(control, 1, r);
        }

        private void LoadData()
        {
            var dt = Db.Query("SELECT * FROM Vehicles WHERE VIN=@vin", new SqlParameter("@vin", _vinKey));
            if (dt.Rows.Count == 0) return; var r = dt.Rows[0];
            _vin.Text = r["VIN"].ToString(); _vin.Enabled = false; _plate.Text = r["license_plate"].ToString(); _power.Value = Convert.ToDecimal(r["engine_power"]);
            _ptsSeries.Text = r["pts_series"].ToString(); _ptsNumber.Text = r["pts_number"].ToString();
            _brand.SelectedValue = Convert.ToInt32(r["id_brand"]);
            HandleBrandChanged();
            _model.SelectedValue = Convert.ToInt32(r["id_model"]);
            HandleModelChanged();
            _category.SelectedValue = Convert.ToInt32(r["id_vehicle_category"]); _client.SelectedValue = Convert.ToInt32(r["id_client"]);
        }

        private void HandleBrandChanged()
        {
            if (_isBindingVehicleFields) return;

            int brandId;
            if (!TryGetSelectedInt(_brand, out brandId)) return;

            _isBindingVehicleFields = true;
            try
            {
                int? oldModelId = GetSelectedInt(_model);
                _model.DataSource = VehicleCatalogService.GetModelsByBrand(brandId);
                _model.ValueMember = "id_model";
                _model.DisplayMember = "model_name";

                if (oldModelId.HasValue)
                    _model.SelectedValue = oldModelId.Value;

                if (_model.SelectedValue == null && _model.Items.Count > 0)
                    _model.SelectedIndex = 0;
            }
            finally
            {
                _isBindingVehicleFields = false;
            }

            HandleModelChanged();
        }

        private void HandleModelChanged()
        {
            if (_isBindingVehicleFields) return;

            int brandId;
            int modelId;
            if (!TryGetSelectedInt(_brand, out brandId) || !TryGetSelectedInt(_model, out modelId)) return;

            var dt = _model.DataSource as DataTable;
            string modelName = string.Empty;
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToInt32(row["id_model"]) == modelId)
                    {
                        modelName = row["model_name"].ToString();
                        break;
                    }
                }
            }

            var categoryTable = _category.DataSource as DataTable;
            int? categoryId = VehicleCatalogService.ResolveCategoryId(brandId, modelId, modelName, categoryTable);
            if (categoryId.HasValue)
                _category.SelectedValue = categoryId.Value;
        }

        private static bool TryGetSelectedInt(ComboBox combo, out int value)
        {
            value = 0;
            if (combo.SelectedValue == null) return false;
            return int.TryParse(combo.SelectedValue.ToString(), out value);
        }

        private static int? GetSelectedInt(ComboBox combo)
        {
            int value;
            return TryGetSelectedInt(combo, out value) ? (int?)value : null;
        }

        private void SaveData()
        {
            try
            {
                if (!ValidateFields()) return;
                string vinText = _vin.Text.Trim().ToUpperInvariant();
                if (!string.IsNullOrEmpty(_vinKey))
                {
                    Db.Execute(@"UPDATE Vehicles SET license_plate=@plate, id_brand=@brand, id_model=@model, id_vehicle_category=@cat, engine_power=@power,
pts_series=@pts_series, pts_number=@pts_number, id_client=@client WHERE VIN=@vin",
                        new SqlParameter("@plate", _plate.Text.Trim()), new SqlParameter("@brand", _brand.SelectedValue), new SqlParameter("@model", _model.SelectedValue),
                        new SqlParameter("@cat", _category.SelectedValue), new SqlParameter("@power", _power.Value), new SqlParameter("@pts_series", _ptsSeries.Text.Trim()),
                        new SqlParameter("@pts_number", _ptsNumber.Text.Trim()), new SqlParameter("@client", _client.SelectedValue), new SqlParameter("@vin", _vinKey));
                }
                else
                {
                    Db.Execute(@"INSERT INTO Vehicles(VIN,license_plate,id_brand,id_model,id_vehicle_category,engine_power,pts_series,pts_number,id_client)
VALUES(@vin,@plate,@brand,@model,@cat,@power,@pts_series,@pts_number,@client)",
                        new SqlParameter("@vin", vinText), new SqlParameter("@plate", _plate.Text.Trim()), new SqlParameter("@brand", _brand.SelectedValue), new SqlParameter("@model", _model.SelectedValue),
                        new SqlParameter("@cat", _category.SelectedValue), new SqlParameter("@power", _power.Value), new SqlParameter("@pts_series", _ptsSeries.Text.Trim()),
                        new SqlParameter("@pts_number", _ptsNumber.Text.Trim()), new SqlParameter("@client", _client.SelectedValue));
                }
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения автомобиля.\n" + ex.Message); }
        }

        private bool ValidateFields()
        {
            var vinText = _vin.Text.Trim().ToUpperInvariant();
            var plateText = _plate.Text.Trim().ToUpperInvariant();
            if (!Regex.IsMatch(vinText, @"^[A-HJ-NPR-Z0-9]{17}$"))
            {
                MessageBox.Show("VIN должен содержать 17 символов (латинские буквы и цифры, без I, O, Q).");
                return false;
            }
            if (!Regex.IsMatch(plateText, @"^[А-ЯA-Z]\d{3}[А-ЯA-Z]{2}\d{2,3}$"))
            {
                MessageBox.Show("Госномер должен быть в формате A123BC77 или A123BC777.");
                return false;
            }
            if (!Regex.IsMatch(_ptsSeries.Text.Trim(), @"^\d{4}$"))
            {
                MessageBox.Show("Серия ПТС должна содержать 4 цифры.");
                return false;
            }
            if (!Regex.IsMatch(_ptsNumber.Text.Trim(), @"^\d{6}$"))
            {
                MessageBox.Show("Номер ПТС должен содержать 6 цифр.");
                return false;
            }
            if (_brand.SelectedValue == null || _model.SelectedValue == null || _category.SelectedValue == null || _client.SelectedValue == null)
            {
                MessageBox.Show("Выберите марку, модель, категорию и владельца.");
                return false;
            }
            if (_power.Value <= 0)
            {
                MessageBox.Show("Мощность должна быть больше 0.");
                return false;
            }
            return true;
        }

        private static void DigitsOnlyKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private static void VinKeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            var c = char.ToUpperInvariant(e.KeyChar);
            if (!char.IsLetterOrDigit(c) || c == 'I' || c == 'O' || c == 'Q')
                e.Handled = true;
        }
    }
}
