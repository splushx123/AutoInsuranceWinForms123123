using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AutoInsuranceWinForms
{
    public static class VehicleCatalogService
    {
        private static readonly Dictionary<string, bool> _columnCache = new Dictionary<string, bool>();

        public static DataTable GetModelsByBrand(int brandId)
        {
            DataTable models;
            if (HasColumn("car_models", "id_brand"))
            {
                models = Db.Query("SELECT id_model, model_name FROM car_models WHERE id_brand=@brand ORDER BY model_name",
                    new SqlParameter("@brand", brandId));
            }
            else
            {
                models = Db.Query("SELECT id_model, model_name FROM car_models WHERE id_model=@brand ORDER BY model_name",
                    new SqlParameter("@brand", brandId));
            }

            if (models.Rows.Count == 0)
                models = Db.Query("SELECT id_model, model_name FROM car_models ORDER BY model_name");

            return models;
        }

        public static int? ResolveCategoryId(int brandId, int modelId, string modelName, DataTable categoryTable)
        {
            if (HasColumn("car_models", "id_vehicle_category"))
            {
                var value = Db.Scalar("SELECT id_vehicle_category FROM car_models WHERE id_model=@model",
                    new SqlParameter("@model", modelId));
                if (value != null && value != DBNull.Value) return Convert.ToInt32(value);
            }

            if (HasColumn("car_brands", "id_vehicle_category"))
            {
                var value = Db.Scalar("SELECT id_vehicle_category FROM car_brands WHERE id_brand=@brand",
                    new SqlParameter("@brand", brandId));
                if (value != null && value != DBNull.Value) return Convert.ToInt32(value);
            }

            return ResolveCategoryIdByFallback(brandId, modelName, categoryTable);
        }

        private static int? ResolveCategoryIdByFallback(int brandId, string modelName, DataTable categoryTable)
        {
            if (categoryTable == null || categoryTable.Rows.Count == 0) return null;

            string name = (modelName ?? string.Empty).Trim().ToUpperInvariant();
            bool isBus = name.Contains("BUS") || name.Contains("URBINO") || name.Contains("CITYLINER")
                         || name.Contains("LOW FLOOR") || name.Contains("S 517") || name.Contains("IKARUS")
                         || name.Contains("MAZ") || name.Contains("YUTONG") || name.Contains("NEOPLAN")
                         || name.Contains("SETRA") || name.Contains("GILLIG") || name.Contains("SOLARIS");
            bool isTruck = name.Contains("TGX") || name.Contains("DAILY") || name.Contains("KAMAZ")
                           || name.Contains("SCANIA") || name.Contains("IVECO") || name.Contains("MAN")
                           || name.Contains("HINO") || name.Contains("TATA") || name.Contains("MAHINDRA")
                           || name.Contains("D-MAX") || name.Contains("1500") || name.Contains("SIERRA")
                           || name.Contains("ZXK") || name.Contains("7530") || name.Contains("5440")
                           || name.Contains("5490") || name.Contains("TX");

            if (!isBus && !isTruck)
            {
                if (brandId >= 80) isBus = true;
                else if (brandId >= 73) isTruck = true;
            }

            if (isBus)
                return FindCategoryId(categoryTable, "АВТОБУС");
            if (isTruck)
                return FindCategoryId(categoryTable, "ГРУЗ");
            return FindCategoryId(categoryTable, "ЛЕГК");
        }

        private static int? FindCategoryId(DataTable categories, string keyword)
        {
            foreach (DataRow row in categories.Rows)
            {
                var categoryName = (row["category_name"].ToString() ?? string.Empty).ToUpperInvariant();
                if (categoryName.Contains(keyword))
                    return Convert.ToInt32(row["id_vehicle_category"]);
            }

            return categories.Rows.Count > 0 ? Convert.ToInt32(categories.Rows[0]["id_vehicle_category"]) : (int?)null;
        }

        private static bool HasColumn(string tableName, string columnName)
        {
            string key = tableName + "." + columnName;
            bool exists;
            if (_columnCache.TryGetValue(key, out exists)) return exists;

            var value = Db.Scalar(@"SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME=@table AND COLUMN_NAME=@column",
                new SqlParameter("@table", tableName),
                new SqlParameter("@column", columnName));

            exists = Convert.ToInt32(value) > 0;
            _columnCache[key] = exists;
            return exists;
        }
    }
}
