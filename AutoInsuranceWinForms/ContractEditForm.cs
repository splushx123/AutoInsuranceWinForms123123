using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class ContractEditForm : Form
    {
        private readonly int? _id;
        private bool _isBindingVehicleFields;
        private Label _existingClientLabel;
        private readonly List<Control> _clientSectionLabels = new List<Control>();
        private readonly List<Control> _clientSectionInputs = new List<Control>();
        private readonly ComboBox _type = Theme.CreateComboBox(220);
        private readonly DateTimePicker _start = Theme.CreateDatePicker(220);
        private readonly DateTimePicker _end = Theme.CreateDatePicker(220);
        private readonly NumericUpDown _amount = Theme.CreateNumeric(220, 100000000);
        private readonly ComboBox _employee = Theme.CreateComboBox(220);
        private readonly NumericUpDown _commission = Theme.CreateNumeric(220, 1000000, 0);
        private readonly ComboBox _vin = Theme.CreateComboBox(220);
        private readonly DateTimePicker _commissionPaymentDate = Theme.CreateDatePicker(220);
        private readonly CheckBox _renewContract = new CheckBox { Text = "Продлить договор", AutoSize = true };
        private readonly ComboBox _existingClient = Theme.CreateComboBox(220);

        private readonly TextBox _clientLastName = Theme.CreateTextBox(220);
        private readonly TextBox _clientFirstName = Theme.CreateTextBox(220);
        private readonly TextBox _clientMiddleName = Theme.CreateTextBox(220);
        private readonly DateTimePicker _clientBirthDate = Theme.CreateDatePicker(220);
        private readonly TextBox _clientPassportSeries = Theme.CreateTextBox(220);
        private readonly TextBox _clientPassportNumber = Theme.CreateTextBox(220);
        private readonly TextBox _clientInn = Theme.CreateTextBox(220);
        private readonly TextBox _clientDriverSeries = Theme.CreateTextBox(220);
        private readonly TextBox _clientPhone = Theme.CreateTextBox(220);
        private readonly TextBox _clientEmail = Theme.CreateTextBox(220);

        private readonly TextBox _vehicleVin = Theme.CreateTextBox(220);
        private readonly TextBox _vehiclePlate = Theme.CreateTextBox(220);
        private readonly ComboBox _vehicleBrand = Theme.CreateComboBox(220);
        private readonly ComboBox _vehicleModel = Theme.CreateComboBox(220);
        private readonly ComboBox _vehicleCategory = Theme.CreateComboBox(220);
        private readonly NumericUpDown _vehiclePower = Theme.CreateNumeric(220, 5000);
        private readonly TextBox _vehiclePtsSeries = Theme.CreateTextBox(220);
        private readonly TextBox _vehiclePtsNumber = Theme.CreateTextBox(220);

        public ContractEditForm(int? id)
        {
            _id = id; Theme.StyleForm(this); Text = id.HasValue ? "Изменение договора" : "Добавление договора"; Width = 780; Height = 700; StartPosition = FormStartPosition.CenterParent;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16), AutoScroll = true };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            AddSection(table, "Договор");
            AddField(table, "Тип страхования", _type); AddField(table, "Дата начала", _start); AddField(table, "Дата окончания", _end); AddField(table, "Страховая сумма", _amount); AddField(table, "Сотрудник", _employee);

            if (id.HasValue)
            {
                AddField(table, "Код комиссии", _commission);
                AddField(table, "Автомобиль", _vin);
            }
            else
            {
                AddField(table, "Продление", _renewContract);
                _existingClientLabel = AddField(table, "Клиент", _existingClient);
                AddField(table, "Дата выплаты комиссии", _commissionPaymentDate);

                AddSection(table, "Клиент");
                AddClientField(table, "Фамилия", _clientLastName);
                AddClientField(table, "Имя", _clientFirstName);
                AddClientField(table, "Отчество", _clientMiddleName);
                AddClientField(table, "Дата рождения", _clientBirthDate);
                AddClientField(table, "Серия паспорта", _clientPassportSeries);
                AddClientField(table, "Номер паспорта", _clientPassportNumber);
                AddClientField(table, "ИНН", _clientInn);
                AddClientField(table, "Серия ВУ", _clientDriverSeries);
                AddClientField(table, "Телефон", _clientPhone);
                AddClientField(table, "E-mail", _clientEmail);

                AddSection(table, "Автомобиль");
                AddField(table, "VIN", _vehicleVin);
                AddField(table, "Госномер", _vehiclePlate);
                AddField(table, "Марка", _vehicleBrand);
                AddField(table, "Модель", _vehicleModel);
                AddField(table, "Категория ТС", _vehicleCategory);
                AddField(table, "Мощность", _vehiclePower);
                AddField(table, "Серия ПТС", _vehiclePtsSeries);
                AddField(table, "Номер ПТС", _vehiclePtsNumber);
            }
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
            var btnSave = Theme.CreatePrimaryButton("Сохранить", 120); btnSave.Click += delegate { SaveData(); };
            var btnCancel = Theme.CreateSecondaryButton("Отмена", 120); btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            buttons.Controls.Add(btnSave); buttons.Controls.Add(btnCancel); Controls.Add(table); Controls.Add(buttons);
            ConfigureInputRules();
            FillCombos(); if (id.HasValue) LoadData();
        }

        private void ConfigureInputRules()
        {
            _clientPassportSeries.MaxLength = 4;
            _clientPassportNumber.MaxLength = 6;
            _clientInn.MaxLength = 12;
            _clientDriverSeries.MaxLength = 4;
            _clientPhone.MaxLength = 18;
            _clientBirthDate.MaxDate = DateTime.Today.AddYears(-18).AddDays(-1);
            _vehicleVin.MaxLength = 17;
            _vehiclePlate.MaxLength = 9;

            _vehicleVin.CharacterCasing = CharacterCasing.Upper;
            _vehiclePlate.CharacterCasing = CharacterCasing.Upper;

            _clientPassportSeries.KeyPress += DigitsOnlyKeyPress;
            _clientPassportNumber.KeyPress += DigitsOnlyKeyPress;
            _clientInn.KeyPress += DigitsOnlyKeyPress;
            _clientDriverSeries.KeyPress += DigitsOnlyKeyPress;
            _vehicleVin.KeyPress += VinKeyPress;
            _end.ValueChanged += delegate { SyncCommissionPaymentDateConstraints(); };
            _vehicleBrand.SelectedValueChanged += delegate { HandleVehicleBrandChanged(); };
            _vehicleModel.SelectedValueChanged += delegate { HandleVehicleModelChanged(); };
            _renewContract.CheckedChanged += delegate { ToggleRenewMode(); };
            _existingClient.SelectedValueChanged += delegate { LoadExistingClientData(); };
            SyncCommissionPaymentDateConstraints();
        }

        private void FillCombos()
        {
            LookupService.Fill(_type, "SELECT id_type, type_name FROM insurance_types ORDER BY type_name", "id_type", "type_name");
            LookupService.Fill(_employee, "SELECT employee_id, last_name + ' ' + first_name AS fio FROM Employees ORDER BY last_name, first_name", "employee_id", "fio");
            LookupService.Fill(_vin, "SELECT VIN, VIN + ' | ' + license_plate AS title FROM Vehicles ORDER BY VIN", "VIN", "title");
            LookupService.Fill(_vehicleBrand, "SELECT id_brand, brand_name FROM car_brands ORDER BY brand_name", "id_brand", "brand_name");
            LookupService.Fill(_vehicleCategory, "SELECT id_vehicle_category, category_name FROM vehicle_categories ORDER BY category_name", "id_vehicle_category", "category_name");
            LookupService.Fill(_existingClient, "SELECT id_client, last_name + ' ' + first_name + ' ' + ISNULL(middle_name,'') AS fio FROM Client ORDER BY last_name, first_name", "id_client", "fio");
            HandleVehicleBrandChanged();
            ToggleRenewMode();
        }

        private Label AddField(TableLayoutPanel t, string name, Control control)
        {
            int r = t.RowCount++;
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            var label = new Label { Text = name, AutoSize = true, Padding = new Padding(0, 9, 0, 0) };
            t.Controls.Add(label, 0, r);
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 4, 0, 4);
            t.Controls.Add(control, 1, r);
            return label;
        }

        private void AddClientField(TableLayoutPanel table, string name, Control input)
        {
            var label = AddField(table, name, input);
            _clientSectionLabels.Add(label);
            _clientSectionInputs.Add(input);
        }

        private void AddSection(TableLayoutPanel t, string title)
        {
            int r = t.RowCount++;
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            var lbl = new Label { Text = title, AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold), Padding = new Padding(0, 10, 0, 0) };
            t.Controls.Add(lbl, 0, r);
            t.SetColumnSpan(lbl, 2);
        }

        private void SyncCommissionPaymentDateConstraints()
        {
            if (_id.HasValue) return;

            _commissionPaymentDate.MinDate = _end.Value.Date;
            if (_commissionPaymentDate.Value.Date < _commissionPaymentDate.MinDate.Date)
                _commissionPaymentDate.Value = _commissionPaymentDate.MinDate.Date;
        }

        private void HandleVehicleBrandChanged()
        {
            if (_isBindingVehicleFields || _id.HasValue) return;

            int brandId;
            if (!TryGetSelectedInt(_vehicleBrand, out brandId)) return;

            _isBindingVehicleFields = true;
            try
            {
                int? oldModelId = GetSelectedInt(_vehicleModel);
                _vehicleModel.DataSource = VehicleCatalogService.GetModelsByBrand(brandId);
                _vehicleModel.ValueMember = "id_model";
                _vehicleModel.DisplayMember = "model_name";

                if (oldModelId.HasValue)
                    _vehicleModel.SelectedValue = oldModelId.Value;

                if (_vehicleModel.SelectedValue == null && _vehicleModel.Items.Count > 0)
                    _vehicleModel.SelectedIndex = 0;
            }
            finally
            {
                _isBindingVehicleFields = false;
            }

            HandleVehicleModelChanged();
        }

        private void HandleVehicleModelChanged()
        {
            if (_isBindingVehicleFields || _id.HasValue) return;

            int brandId;
            int modelId;
            if (!TryGetSelectedInt(_vehicleBrand, out brandId) || !TryGetSelectedInt(_vehicleModel, out modelId)) return;

            var dt = _vehicleModel.DataSource as DataTable;
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

            var categoryTable = _vehicleCategory.DataSource as DataTable;
            int? categoryId = VehicleCatalogService.ResolveCategoryId(brandId, modelId, modelName, categoryTable);
            if (categoryId.HasValue)
                _vehicleCategory.SelectedValue = categoryId.Value;
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

        private void ToggleRenewMode()
        {
            if (_id.HasValue) return;

            bool isRenew = _renewContract.Checked;
            _existingClient.Enabled = isRenew;
            _existingClient.Visible = isRenew;
            if (_existingClientLabel != null) _existingClientLabel.Visible = isRenew;

            foreach (var label in _clientSectionLabels) label.Visible = !isRenew;
            foreach (var input in _clientSectionInputs) input.Visible = !isRenew;

            _clientLastName.ReadOnly = isRenew;
            _clientFirstName.ReadOnly = isRenew;
            _clientMiddleName.ReadOnly = isRenew;
            _clientBirthDate.Enabled = !isRenew;
            _clientPassportSeries.ReadOnly = isRenew;
            _clientPassportNumber.ReadOnly = isRenew;
            _clientInn.ReadOnly = isRenew;
            _clientDriverSeries.ReadOnly = isRenew;
            _clientPhone.ReadOnly = isRenew;
            _clientEmail.ReadOnly = isRenew;

            if (isRenew) LoadExistingClientData();
        }

        private void LoadExistingClientData()
        {
            if (_id.HasValue || !_renewContract.Checked || _existingClient.SelectedValue == null) return;

            var dt = Db.Query("SELECT * FROM Client WHERE id_client=@id", new SqlParameter("@id", _existingClient.SelectedValue));
            if (dt.Rows.Count == 0) return;
            var r = dt.Rows[0];
            _clientLastName.Text = r["last_name"].ToString();
            _clientFirstName.Text = r["first_name"].ToString();
            _clientMiddleName.Text = r["middle_name"].ToString();
            if (r["birth_date"] != DBNull.Value) SetDatePickerValueSafe(_clientBirthDate, Convert.ToDateTime(r["birth_date"]));
            _clientPassportSeries.Text = r["passport_series"].ToString();
            _clientPassportNumber.Text = r["passport_number"].ToString();
            _clientInn.Text = r["inn"].ToString();
            _clientDriverSeries.Text = r["drivers_license_series"].ToString();
            _clientPhone.Text = r["phone"].ToString();
            _clientEmail.Text = r["email"].ToString();
        }

        private static void SetDatePickerValueSafe(DateTimePicker picker, DateTime value)
        {
            var date = value.Date;
            if (date < picker.MinDate.Date) date = picker.MinDate.Date;
            if (date > picker.MaxDate.Date) date = picker.MaxDate.Date;
            picker.Value = date;
        }

        private void LoadData()
        {
            var dt = Db.Query("SELECT * FROM Contract WHERE id_contract=@id", new SqlParameter("@id", _id.Value)); if (dt.Rows.Count == 0) return; DataRow r = dt.Rows[0];
            _type.SelectedValue = Convert.ToInt32(r["id_type"]); _start.Value = Convert.ToDateTime(r["start_date"]); _end.Value = Convert.ToDateTime(r["end_date"]); _amount.Value = Convert.ToDecimal(r["insurance_amount"]);
            _employee.SelectedValue = Convert.ToInt32(r["employee_id"]); _commission.Value = Convert.ToDecimal(r["id_commission"]); _vin.SelectedValue = r["VIN"].ToString();
        }

        private void SaveData()
        {
            try
            {
                if (!ValidateCommonFields()) return;

                if (_id.HasValue)
                {
                    Db.Execute(@"UPDATE Contract SET id_type=@type, start_date=@start, end_date=@end, insurance_amount=@amount, employee_id=@employee, id_commission=@commission, VIN=@vin WHERE id_contract=@id",
                        new SqlParameter("@type", _type.SelectedValue), new SqlParameter("@start", _start.Value.Date), new SqlParameter("@end", _end.Value.Date), new SqlParameter("@amount", _amount.Value), new SqlParameter("@employee", _employee.SelectedValue), new SqlParameter("@commission", Convert.ToInt32(_commission.Value)), new SqlParameter("@vin", _vin.SelectedValue), new SqlParameter("@id", _id.Value));
                }
                else
                {
                    if (!ValidateAddModeFields()) return;
                    string vin = _vehicleVin.Text.Trim().ToUpperInvariant();

                    using (var connection = new SqlConnection(Db.ConnectionString))
                    {
                        connection.Open();
                        using (var tx = connection.BeginTransaction())
                        {
                            try
                            {
                                int clientId = _renewContract.Checked ? Convert.ToInt32(_existingClient.SelectedValue) : NextId(connection, tx, "Client", "id_client");
                                int commissionId = NextId(connection, tx, "commissions", "commission_id");
                                int contractId = NextId(connection, tx, "Contract", "id_contract");

                                if (ExistsInTransaction(connection, tx, "SELECT COUNT(1) FROM Vehicles WHERE VIN=@vin", new SqlParameter("@vin", vin)))
                                    throw new Exception("Автомобиль с таким VIN уже существует.");

                                if (!_renewContract.Checked)
                                {
                                    ExecuteInTransaction(connection, tx, @"INSERT INTO Client(id_client,last_name,first_name,middle_name,birth_date,passport_series,passport_number,inn,drivers_license_series,phone,email)
VALUES(@id,@last_name,@first_name,@middle_name,@birth_date,@passport_series,@passport_number,@inn,@drivers_license_series,@phone,@email)",
                                    new SqlParameter("@id", clientId),
                                    new SqlParameter("@last_name", _clientLastName.Text.Trim()),
                                    new SqlParameter("@first_name", _clientFirstName.Text.Trim()),
                                    new SqlParameter("@middle_name", _clientMiddleName.Text.Trim()),
                                    new SqlParameter("@birth_date", _clientBirthDate.Value.Date),
                                    new SqlParameter("@passport_series", _clientPassportSeries.Text.Trim()),
                                    new SqlParameter("@passport_number", _clientPassportNumber.Text.Trim()),
                                    new SqlParameter("@inn", _clientInn.Text.Trim()),
                                    new SqlParameter("@drivers_license_series", _clientDriverSeries.Text.Trim()),
                                    new SqlParameter("@phone", NormalizeClientPhone()),
                                    new SqlParameter("@email", _clientEmail.Text.Trim()));
                                }

                                ExecuteInTransaction(connection, tx, @"INSERT INTO Vehicles(VIN,license_plate,id_brand,id_model,id_vehicle_category,engine_power,pts_series,pts_number,id_client)
VALUES(@vin,@plate,@brand,@model,@cat,@power,@pts_series,@pts_number,@client)",
                                    new SqlParameter("@vin", vin),
                                    new SqlParameter("@plate", _vehiclePlate.Text.Trim()),
                                    new SqlParameter("@brand", _vehicleBrand.SelectedValue),
                                    new SqlParameter("@model", _vehicleModel.SelectedValue),
                                    new SqlParameter("@cat", _vehicleCategory.SelectedValue),
                                    new SqlParameter("@power", _vehiclePower.Value),
                                    new SqlParameter("@pts_series", _vehiclePtsSeries.Text.Trim()),
                                    new SqlParameter("@pts_number", _vehiclePtsNumber.Text.Trim()),
                                    new SqlParameter("@client", clientId));

                                ExecuteInTransaction(connection, tx, "INSERT INTO commissions(commission_id,payment_date) VALUES(@id,@date)",
                                    new SqlParameter("@id", commissionId),
                                    new SqlParameter("@date", _commissionPaymentDate.Value.Date));

                                ExecuteInTransaction(connection, tx, @"INSERT INTO Contract(id_contract,id_type,start_date,end_date,insurance_amount,employee_id,id_commission,VIN)
VALUES(@id,@type,@start,@end,@amount,@employee,@commission,@vin)",
                                    new SqlParameter("@id", contractId),
                                    new SqlParameter("@type", _type.SelectedValue),
                                    new SqlParameter("@start", _start.Value.Date),
                                    new SqlParameter("@end", _end.Value.Date),
                                    new SqlParameter("@amount", _amount.Value),
                                    new SqlParameter("@employee", _employee.SelectedValue),
                                    new SqlParameter("@commission", commissionId),
                                    new SqlParameter("@vin", vin));

                                tx.Commit();
                            }
                            catch
                            {
                                tx.Rollback();
                                throw;
                            }
                        }
                    }
                }
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения договора.\n" + ex.Message); }
        }

        private bool ValidateCommonFields()
        {
            if (_type.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип страхования.");
                return false;
            }
            if (_employee.SelectedValue == null)
            {
                MessageBox.Show("Выберите сотрудника.");
                return false;
            }
            if (_end.Value.Date < _start.Value.Date)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала.");
                return false;
            }
            if (_amount.Value <= 0)
            {
                MessageBox.Show("Страховая сумма должна быть больше 0.");
                return false;
            }
            if (_id.HasValue && _vin.SelectedValue == null)
            {
                MessageBox.Show("Выберите автомобиль.");
                return false;
            }
            if (_id.HasValue && _commission.Value <= 0)
            {
                MessageBox.Show("Код комиссии должен быть больше 0.");
                return false;
            }
            return true;
        }

        private bool ValidateAddModeFields()
        {
            bool isRenew = _renewContract.Checked;
            if (isRenew && _existingClient.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента для продления договора.");
                return false;
            }

            if (!isRenew)
            {
                if (string.IsNullOrWhiteSpace(_clientLastName.Text) || string.IsNullOrWhiteSpace(_clientFirstName.Text))
                {
                    MessageBox.Show("Укажите фамилию и имя клиента.");
                    return false;
                }
                if (_clientBirthDate.Value.Date > DateTime.Today)
                {
                    MessageBox.Show("Дата рождения клиента не может быть в будущем.");
                    return false;
                }
                if (!ValidationRules.IsOlderThanYears(_clientBirthDate.Value.Date, 18))
                {
                    MessageBox.Show("Клиент должен быть старше 18 лет.");
                    return false;
                }
            }
            if (_commissionPaymentDate.Value.Date < _end.Value.Date)
            {
                MessageBox.Show("Дата выплаты комиссии не может быть раньше даты окончания договора.");
                return false;
            }

            if (!isRenew)
            {
                if (string.IsNullOrWhiteSpace(_clientLastName.Text) || string.IsNullOrWhiteSpace(_clientFirstName.Text))
                {
                    MessageBox.Show("Укажите фамилию и имя клиента.");
                    return false;
                }
                if (_clientBirthDate.Value.Date > DateTime.Today)
                {
                    MessageBox.Show("Дата рождения клиента не может быть в будущем.");
                    return false;
                }
                if (!ValidationRules.IsOlderThanYears(_clientBirthDate.Value.Date, 18))
                {
                    MessageBox.Show("Клиент должен быть старше 18 лет.");
                    return false;
                }
            }
            if (_commissionPaymentDate.Value.Date < _end.Value.Date)
            {
                MessageBox.Show("Дата выплаты комиссии не может быть раньше даты окончания договора.");
                return false;
            }
            if (!isRenew && (string.IsNullOrWhiteSpace(_clientPhone.Text) || string.IsNullOrWhiteSpace(_clientEmail.Text)))
            {
                MessageBox.Show("Дата рождения клиента не может быть в будущем.");
                return false;
            }
            if (!ValidationRules.IsOlderThanYears(_clientBirthDate.Value.Date, 18))
            {
                MessageBox.Show("Клиент должен быть старше 18 лет.");
                return false;
            }
            if (!isRenew)
            {
                if (!Regex.IsMatch(_clientPassportSeries.Text.Trim(), @"^\d{4}$"))
                {
                    MessageBox.Show("Серия паспорта должна содержать ровно 4 цифры.");
                    return false;
                }
                if (!Regex.IsMatch(_clientPassportNumber.Text.Trim(), @"^\d{6}$"))
                {
                    MessageBox.Show("Номер паспорта должен содержать ровно 6 цифр.");
                    return false;
                }
                if (!Regex.IsMatch(_clientInn.Text.Trim(), @"^\d{10}(\d{2})?$"))
                {
                    MessageBox.Show("ИНН должен содержать 10 или 12 цифр.");
                    return false;
                }
                if (!Regex.IsMatch(_clientDriverSeries.Text.Trim(), @"^\d{4}$"))
                {
                    MessageBox.Show("Серия ВУ должна содержать ровно 4 цифры.");
                    return false;
                }

                if (!ValidationRules.TryNormalizePhoneRu(_clientPhone.Text, out _))
                {
                    MessageBox.Show("Телефон клиента должен быть в формате +7XXXXXXXXXX (10 цифр после +7).");
                    return false;
                }

                var email = _clientEmail.Text.Trim();
                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    MessageBox.Show("Введите корректный E-mail.");
                    return false;
                }
            }

            var vin = _vehicleVin.Text.Trim().ToUpperInvariant();
            if (!Regex.IsMatch(vin, @"^[A-HJ-NPR-Z0-9]{17}$"))
            {
                MessageBox.Show("VIN должен содержать ровно 17 символов (латинские буквы и цифры, без I, O, Q).");
                return false;
            }

            var plate = _vehiclePlate.Text.Trim().ToUpperInvariant();
            if (!Regex.IsMatch(plate, @"^[А-ЯA-Z]\d{3}[А-ЯA-Z]{2}\d{2,3}$"))
            {
                MessageBox.Show("Госномер должен быть в формате A123BC77 или A123BC777.");
                return false;
            }
            return true;
        }

        private string NormalizeClientPhone()
        {
            string normalizedPhone;
            if (!ValidationRules.TryNormalizePhoneRu(_clientPhone.Text, out normalizedPhone))
                throw new Exception("Некорректный телефон клиента.");
            return normalizedPhone;
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

        private static int NextId(SqlConnection connection, SqlTransaction tx, string tableName, string idField)
        {
            using (var cmd = new SqlCommand(string.Format("SELECT ISNULL(MAX({0}), 0) + 1 FROM {1}", idField, tableName), connection, tx))
                return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static void ExecuteInTransaction(SqlConnection connection, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(sql, connection, tx))
            {
                if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
            }
        }

        private static bool ExistsInTransaction(SqlConnection connection, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(sql, connection, tx))
            {
                if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(parameters);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}
