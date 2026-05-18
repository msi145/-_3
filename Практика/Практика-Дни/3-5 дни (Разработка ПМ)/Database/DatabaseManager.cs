using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using WpfApp1.Models;
using System.Data;

namespace WpfApp1
{
    public static class DatabaseManager
    {
        private static string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=WebApplication2Context-20260507124423;Trusted_Connection=True;Encrypt=False;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static void InitializeDatabase()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string createCaptchaTable = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='captcha_attempts' AND xtype='U')
                        CREATE TABLE captcha_attempts (
                            id INT IDENTITY(1,1) PRIMARY KEY,
                            session_id NVARCHAR(100) NOT NULL,
                            captcha_text NVARCHAR(10) NOT NULL,
                            created_at DATETIME NOT NULL,
                            is_used BIT DEFAULT 0
                        )";

                    using (var cmd = new SqlCommand(createCaptchaTable, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
            }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        public static async Task<User> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT u.*, r.Name as RoleName, r.Description as RoleDescription, 
                               d.Name as DepartmentName, d.Description as DepartmentDescription
                        FROM users u
                        LEFT JOIN roles r ON u.role_id = r.id
                        LEFT JOIN departments d ON u.department_id = d.id
                        WHERE u.username = @username AND u.is_active = 1";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var user = new User
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Username = reader["username"]?.ToString() ?? "",
                                    PasswordHash = reader["password_hash"]?.ToString() ?? "",
                                    FullName = reader["full_name"]?.ToString() ?? "",
                                    Email = reader["email"]?.ToString() ?? "",
                                    RoleId = reader["role_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("role_id")) : (int?)null,
                                    DepartmentId = reader["department_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("department_id")) : (int?)null,
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
                                    LastLoginAt = reader["last_login_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("last_login_at")) : (DateTime?)null,
                                    CreatedAt = reader["created_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("created_at")) : (DateTime?)null
                                };

                                if (VerifyPassword(password, user.PasswordHash))
                                {
                                    await UpdateLastLoginAsync(user.Id);
                                    return user;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auth error: {ex.Message}");
            }
            return null;
        }

        private static async Task UpdateLastLoginAsync(int userId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "UPDATE users SET last_login_at = @now WHERE id = @userId";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update last login error: {ex.Message}");
            }
        }

        #region Products

        public static async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT * FROM products ORDER BY name";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Name = reader["name"]?.ToString() ?? "",
                                ProductType = reader["product_type"]?.ToString() ?? "",
                                ReleaseForm = reader["release_form"]?.ToString() ?? "",
                                Status = reader["status"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get products error: {ex.Message}");
            }
            return products;
        }

        public static async Task<int> AddProductAsync(Product product)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        INSERT INTO products (name, product_type, release_form, status) 
                        VALUES (@name, @type, @releaseForm, @status);
                        SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", product.Name ?? "");
                        cmd.Parameters.AddWithValue("@type", product.ProductType ?? "");
                        cmd.Parameters.AddWithValue("@releaseForm", product.ReleaseForm ?? "");
                        cmd.Parameters.AddWithValue("@status", product.Status ?? "active");

                        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add product error: {ex.Message}");
                return 0;
            }
        }

        public static async Task UpdateProductAsync(Product product)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        UPDATE products 
                        SET name = @name, product_type = @type, release_form = @releaseForm, status = @status
                        WHERE id = @id";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", product.Id);
                        cmd.Parameters.AddWithValue("@name", product.Name ?? "");
                        cmd.Parameters.AddWithValue("@type", product.ProductType ?? "");
                        cmd.Parameters.AddWithValue("@releaseForm", product.ReleaseForm ?? "");
                        cmd.Parameters.AddWithValue("@status", product.Status ?? "");

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update product error: {ex.Message}");
            }
        }

        public static async Task DeleteProductAsync(int id)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "DELETE FROM products WHERE id = @id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete product error: {ex.Message}");
            }
        }

        #endregion
        #region Step Executions (continue)

        public static async Task CompleteStepExecutionAsync(int stepExecutionId, int userId, decimal? actualValue = null, int? actualDuration = null, string comment = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                UPDATE step_executions 
                SET status = 'completed', finished_at = @now, finished_by = @userId,
                    actual_param_value = @actualValue, actual_duration_min = @actualDuration, comment = @comment
                WHERE id = @stepId";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@actualValue", actualValue ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@actualDuration", actualDuration ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@comment", comment ?? "");
                        cmd.Parameters.AddWithValue("@stepId", stepExecutionId);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Complete step execution error: {ex.Message}");
                throw;
            }
        }

        #endregion
        #region Recipes

        public static async Task<List<Recipe>> GetAllRecipesAsync()
        {
            var recipes = new List<Recipe>();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT r.*, p.name as ProductName 
                        FROM recipes r
                        LEFT JOIN products p ON r.product_id = p.id
                        ORDER BY r.id DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            recipes.Add(new Recipe
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
                                Version = reader.GetInt32(reader.GetOrdinal("version")),
                                Status = reader["status"]?.ToString() ?? "",
                                Description = reader["description"]?.ToString() ?? "",
                                CreatedBy = reader["created_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("created_by")) : (int?)null,
                                ApprovedBy = reader["approved_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("approved_by")) : (int?)null,
                                ApprovedAt = reader["approved_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("approved_at")) : (DateTime?)null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get recipes error: {ex.Message}");
            }
            return recipes;
        }

        public static async Task<Recipe> GetRecipeWithComponentsAsync(int recipeId)
        {
            Recipe recipe = null;
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();

                    string recipeQuery = "SELECT * FROM recipes WHERE id = @id";
                    using (var cmd = new SqlCommand(recipeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", recipeId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                recipe = new Recipe
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
                                    Version = reader.GetInt32(reader.GetOrdinal("version")),
                                    Status = reader["status"]?.ToString() ?? "",
                                    Description = reader["description"]?.ToString() ?? "",
                                    CreatedBy = reader["created_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("created_by")) : (int?)null,
                                    ApprovedBy = reader["approved_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("approved_by")) : (int?)null,
                                    ApprovedAt = reader["approved_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("approved_at")) : (DateTime?)null,
                                    Components = new List<RecipeComponent>()
                                };
                            }
                        }
                    }

                    if (recipe != null)
                    {
                        string compQuery = @"
                    SELECT rc.*, rm.name as MaterialName, rm.unit as MaterialUnit
                    FROM recipe_components rc
                    LEFT JOIN raw_materials rm ON rc.material_id = rm.id
                    WHERE rc.recipe_id = @recipeId
                    ORDER BY rc.load_order";

                        using (var cmd = new SqlCommand(compQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@recipeId", recipeId);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    recipe.Components.Add(new RecipeComponent
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                                        RecipeId = reader.GetInt32(reader.GetOrdinal("recipe_id")),
                                        MaterialId = reader.GetInt32(reader.GetOrdinal("material_id")),
                                        Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
                                        Unit = reader["unit"]?.ToString() ?? "",
                                        TolerancePct = reader["tolerance_pct"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("tolerance_pct")) : (decimal?)null,
                                        LoadOrder = reader["load_order"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("load_order")) : (int?)null,
                                        MaterialName = reader["MaterialName"]?.ToString() ?? "" // Добавьте эту строку
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get recipe with components error: {ex.Message}");
            }
            return recipe;
        }

        #endregion

        #region Production Orders

        public static async Task<List<ProductionOrder>> GetAllProductionOrdersAsync()
        {
            var orders = new List<ProductionOrder>();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT po.*, p.name as ProductName, r.version as RecipeVersion
                        FROM production_orders po
                        LEFT JOIN products p ON po.product_id = p.id
                        LEFT JOIN recipes r ON po.recipe_id = r.id
                        ORDER BY po.planned_date DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new ProductionOrder
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                OrderNumber = reader["order_number"]?.ToString() ?? "",
                                ProductId = reader["product_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("product_id")) : (int?)null,
                                RecipeId = reader["recipe_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("recipe_id")) : (int?)null,
                                TechMapId = reader["tech_map_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("tech_map_id")) : (int?)null,
                                PlannedQty = reader["planned_qty"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("planned_qty")) : (decimal?)null,
                                QtyUnit = reader["qty_unit"]?.ToString() ?? "",
                                PlannedDate = reader.GetDateTime(reader.GetOrdinal("planned_date")),
                                Status = reader["status"]?.ToString() ?? "",
                                Priority = reader["priority"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("priority")) : (int?)null,
                                CreatedBy = reader["created_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("created_by")) : (int?)null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get production orders error: {ex.Message}");
            }
            return orders;
        }

        public static async Task<int> AddProductionOrderAsync(ProductionOrder order)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        INSERT INTO production_orders (order_number, product_id, recipe_id, tech_map_id, planned_qty, qty_unit, planned_date, status, priority, created_by)
                        VALUES (@orderNumber, @productId, @recipeId, @techMapId, @plannedQty, @qtyUnit, @plannedDate, @status, @priority, @createdBy);
                        SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderNumber", order.OrderNumber ?? GenerateOrderNumber());
                        cmd.Parameters.AddWithValue("@productId", order.ProductId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@recipeId", order.RecipeId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@techMapId", order.TechMapId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@plannedQty", order.PlannedQty ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@qtyUnit", order.QtyUnit ?? "kg");
                        cmd.Parameters.AddWithValue("@plannedDate", order.PlannedDate);
                        cmd.Parameters.AddWithValue("@status", order.Status ?? "planned");
                        cmd.Parameters.AddWithValue("@priority", order.Priority ?? 1);
                        cmd.Parameters.AddWithValue("@createdBy", order.CreatedBy ?? (object)DBNull.Value);

                        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add production order error: {ex.Message}");
                return 0;
            }
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        #endregion

        #region Production Batches

        public static async Task<List<ProductionBatch>> GetBatchesByOrderAsync(int orderId)
        {
            var batches = new List<ProductionBatch>();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT pb.*, u.full_name as ResponsibleName
                        FROM production_batches pb
                        LEFT JOIN users u ON pb.responsible_user = u.id
                        WHERE pb.order_id = @orderId
                        ORDER BY pb.started_at DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                batches.Add(new ProductionBatch
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    OrderId = reader["order_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("order_id")) : (int?)null,
                                    BatchNumber = reader["batch_number"]?.ToString() ?? "",
                                    Status = reader["status"]?.ToString() ?? "",
                                    ActualQty = reader["actual_qty"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("actual_qty")) : (decimal?)null,
                                    QtyUnit = reader["qty_unit"]?.ToString() ?? "",
                                    StartedAt = reader["started_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("started_at")) : (DateTime?)null,
                                    FinishedAt = reader["finished_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("finished_at")) : (DateTime?)null,
                                    ResponsibleUser = reader["responsible_user"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("responsible_user")) : (int?)null,
                                    Notes = reader["notes"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get batches by order error: {ex.Message}");
            }
            return batches;
        }

        public static async Task<int> CreateBatchAsync(ProductionBatch batch)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        INSERT INTO production_batches (order_id, batch_number, status, qty_unit, responsible_user, notes)
                        VALUES (@orderId, @batchNumber, @status, @qtyUnit, @responsibleUser, @notes);
                        SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", batch.OrderId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@batchNumber", batch.BatchNumber ?? GenerateBatchNumber());
                        cmd.Parameters.AddWithValue("@status", "created");
                        cmd.Parameters.AddWithValue("@qtyUnit", batch.QtyUnit ?? "kg");
                        cmd.Parameters.AddWithValue("@responsibleUser", batch.ResponsibleUser ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@notes", batch.Notes ?? "");

                        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create batch error: {ex.Message}");
                return 0;
            }
        }

        private static string GenerateBatchNumber()
        {
            return $"BATCH-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(100, 999)}";
        }

        public static async Task StartBatchAsync(int batchId, int userId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        UPDATE production_batches 
                        SET status = 'in_progress', started_at = @now, responsible_user = @userId
                        WHERE id = @batchId";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Start batch error: {ex.Message}");
            }
        }

        public static async Task CompleteBatchAsync(int batchId, decimal actualQty)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        UPDATE production_batches 
                        SET status = 'completed', finished_at = @now, actual_qty = @actualQty
                        WHERE id = @batchId";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@actualQty", actualQty);
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Complete batch error: {ex.Message}");
            }
        }

        #endregion

        #region Step Executions

        public static async Task<List<StepExecution>> GetBatchStepExecutionsAsync(int batchId)
        {
            var executions = new List<StepExecution>();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT se.*, ts.name as StepName, ts.step_number, e.name as EquipmentName,
                               u1.full_name as StartedByName, u2.full_name as FinishedByName
                        FROM step_executions se
                        LEFT JOIN tech_steps ts ON se.tech_step_id = ts.id
                        LEFT JOIN equipments e ON se.equipment_id = e.id
                        LEFT JOIN users u1 ON se.started_by = u1.id
                        LEFT JOIN users u2 ON se.finished_by = u2.id
                        WHERE se.batch_id = @batchId
                        ORDER BY ts.step_number";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                executions.Add(new StepExecution
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    BatchId = reader.GetInt32(reader.GetOrdinal("batch_id")),
                                    TechStepId = reader.GetInt32(reader.GetOrdinal("tech_step_id")),
                                    Status = reader["status"]?.ToString() ?? "",
                                    StartedAt = reader["started_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("started_at")) : (DateTime?)null,
                                    FinishedAt = reader["finished_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("finished_at")) : (DateTime?)null,
                                    StartedBy = reader["started_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("started_by")) : (int?)null,
                                    FinishedBy = reader["finished_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("finished_by")) : (int?)null,
                                    ActualParamValue = reader["actual_param_value"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("actual_param_value")) : (decimal?)null,
                                    ActualDurationMin = reader["actual_duration_min"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("actual_duration_min")) : (int?)null,
                                    Comment = reader["comment"]?.ToString() ?? "",
                                    EquipmentId = reader["equipment_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("equipment_id")) : (int?)null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get step executions error: {ex.Message}");
            }
            return executions;
        }

        #endregion

        #region Deviations

        public static async Task<List<Deviation>> GetBatchDeviationsAsync(int batchId)
        {
            var deviations = new List<Deviation>();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT d.*, u1.full_name as RegisteredByName, u2.full_name as ResolvedByName
                        FROM deviations d
                        LEFT JOIN users u1 ON d.registered_by = u1.id
                        LEFT JOIN users u2 ON d.resolved_by = u2.id
                        WHERE d.batch_id = @batchId
                        ORDER BY d.registered_at DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                deviations.Add(new Deviation
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    BatchId = reader.GetInt32(reader.GetOrdinal("batch_id")),
                                    StepExecutionId = reader["step_execution_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("step_execution_id")) : (int?)null,
                                    Severity = reader["severity"]?.ToString() ?? "",
                                    DeviationType = reader["deviation_type"]?.ToString() ?? "",
                                    Description = reader["description"]?.ToString() ?? "",
                                    RegisteredBy = reader["registered_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("registered_by")) : (int?)null,
                                    RegisteredAt = reader["registered_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("registered_at")) : (DateTime?)null,
                                    ResolutionStatus = reader["resolution_status"]?.ToString() ?? "",
                                    ResolvedBy = reader["resolved_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("resolved_by")) : (int?)null,
                                    ResolvedAt = reader["resolved_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("resolved_at")) : (DateTime?)null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get deviations error: {ex.Message}");
            }
            return deviations;
        }

        public static async Task<int> AddDeviationAsync(Deviation deviation)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        INSERT INTO deviations (batch_id, step_execution_id, severity, deviation_type, description, registered_by, registered_at, resolution_status)
                        VALUES (@batchId, @stepExecId, @severity, @type, @description, @registeredBy, @now, 'open');
                        SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@batchId", deviation.BatchId);
                        cmd.Parameters.AddWithValue("@stepExecId", deviation.StepExecutionId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@severity", deviation.Severity ?? "medium");
                        cmd.Parameters.AddWithValue("@type", deviation.DeviationType ?? "");
                        cmd.Parameters.AddWithValue("@description", deviation.Description ?? "");
                        cmd.Parameters.AddWithValue("@registeredBy", deviation.RegisteredBy ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);

                        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add deviation error: {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region Tech Maps

        public static async Task<List<TechMap>> GetAllTechMapsAsync()
        {
            var techMaps = new List<TechMap>();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                        SELECT tm.*, r.version as RecipeVersion, e.name as EquipmentName
                        FROM tech_maps tm
                        LEFT JOIN recipes r ON tm.recipe_id = r.id
                        LEFT JOIN equipments e ON tm.equipment_id = e.id
                        ORDER BY tm.id DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            techMaps.Add(new TechMap
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                RecipeId = reader.GetInt32(reader.GetOrdinal("recipe_id")),
                                Name = reader["name"]?.ToString() ?? "",
                                Version = reader.GetInt32(reader.GetOrdinal("version")),
                                Status = reader["status"]?.ToString() ?? "",
                                EquipmentId = reader["equipment_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("equipment_id")) : (int?)null,
                                CreatedBy = reader["created_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("created_by")) : (int?)null,
                                ApprovedBy = reader["approved_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("approved_by")) : (int?)null,
                                ApprovedAt = reader["approved_at"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("approved_at")) : (DateTime?)null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get tech maps error: {ex.Message}");
            }
            return techMaps;
        }

        #endregion

        #region CAPTCHA

        private static readonly Dictionary<string, CaptchaInfo> _captchaStore = new Dictionary<string, CaptchaInfo>();

        public class CaptchaInfo
        {
            public string Text { get; set; }
            public DateTime CreatedAt { get; set; }
            public byte[] ImageData { get; set; }
        }

        public static CaptchaInfo GenerateCaptcha(string sessionId)
        {
            var random = new Random();
            string captchaText = random.Next(1000, 9999).ToString();

            int width = 150;
            int height = 50;

            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.WhiteSmoke);

                for (int i = 0; i < 10; i++)
                {
                    using (var pen = new Pen(Color.FromArgb(random.Next(100, 200), random.Next(100, 200), random.Next(100, 200))))
                    {
                        graphics.DrawLine(pen, random.Next(width), random.Next(height), random.Next(width), random.Next(height));
                    }
                }

                for (int i = 0; i < 200; i++)
                {
                    bitmap.SetPixel(random.Next(width), random.Next(height), Color.FromArgb(random.Next(100, 200), random.Next(100, 200), random.Next(100, 200)));
                }

                using (var font = new Font("Arial", 22, FontStyle.Bold))
                {
                    for (int i = 0; i < captchaText.Length; i++)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(random.Next(50, 150), random.Next(50, 150), random.Next(50, 150))))
                        {
                            float x = 20 + (i * 25) + random.Next(-3, 3);
                            float y = 8 + random.Next(-5, 5);
                            float angle = random.Next(-15, 15);

                            graphics.TranslateTransform(x, y);
                            graphics.RotateTransform(angle);
                            graphics.DrawString(captchaText[i].ToString(), font, brush, 0, 0);
                            graphics.RotateTransform(-angle);
                            graphics.TranslateTransform(-x, -y);
                        }
                    }
                }

                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    var captcha = new CaptchaInfo
                    {
                        Text = captchaText,
                        CreatedAt = DateTime.Now,
                        ImageData = ms.ToArray()
                    };

                    lock (_captchaStore)
                    {
                        _captchaStore[sessionId] = captcha;

                        var oldKeys = _captchaStore.Where(kv => kv.Value.CreatedAt < DateTime.Now.AddMinutes(-5)).Select(kv => kv.Key).ToList();
                        foreach (var key in oldKeys)
                        {
                            _captchaStore.Remove(key);
                        }
                    }

                    return captcha;
                }
            }
        }

        public static bool VerifyCaptcha(string sessionId, string userInput)
        {
            lock (_captchaStore)
            {
                if (_captchaStore.ContainsKey(sessionId))
                {
                    bool isValid = _captchaStore[sessionId].Text == userInput;
                    _captchaStore.Remove(sessionId);
                    return isValid;
                }
                return false;
            }
        }

        #endregion
        #region Raw Materials (добавьте эти методы)

        public static async Task<List<RawMaterial>> GetAllRawMaterialsAsync()
        {
            var materials = new List<RawMaterial>();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT * FROM raw_materials ORDER BY name";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            materials.Add(new RawMaterial
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Name = reader["name"]?.ToString() ?? "",
                                Unit = reader["unit"]?.ToString() ?? "",
                                MaterialType = reader["material_type"]?.ToString() ?? "",
                                CasNumber = reader["cas_number"]?.ToString() ?? "",
                                Specification = reader["specification"]?.ToString() ?? "",
                                IsHazardous = reader["is_hazardous"] != DBNull.Value && reader.GetBoolean(reader.GetOrdinal("is_hazardous"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get raw materials error: {ex.Message}");
            }
            return materials;
        }

        public static async Task<List<RawMaterialBatch>> GetRawMaterialBatchesAsync(int? materialId = null)
        {
            var batches = new List<RawMaterialBatch>();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                SELECT rmb.*, rm.name as MaterialName, s.name as SupplierName
                FROM raw_material_batches rmb
                LEFT JOIN raw_materials rm ON rmb.material_id = rm.id
                LEFT JOIN suppliers s ON rmb.supplier_id = s.id
                WHERE (@materialId IS NULL OR rmb.material_id = @materialId)
                ORDER BY rmb.receipt_date DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@materialId", materialId ?? (object)DBNull.Value);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                batches.Add(new RawMaterialBatch
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    MaterialId = reader.GetInt32(reader.GetOrdinal("material_id")),
                                    SupplierId = reader["supplier_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("supplier_id")) : (int?)null,
                                    BatchNumber = reader["batch_number"]?.ToString() ?? "",
                                    ReceiptDate = reader.GetDateTime(reader.GetOrdinal("receipt_date")),
                                    ExpiryDate = reader["expiry_date"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("expiry_date")) : (DateTime?)null,
                                    QuantityKg = reader.GetDecimal(reader.GetOrdinal("quantity_kg")),
                                    RemainingQty = reader.GetDecimal(reader.GetOrdinal("remaining_qty")),
                                    Status = reader["status"]?.ToString() ?? "",
                                    CertificateRef = reader["certificate_ref"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get raw material batches error: {ex.Message}");
            }
            return batches;
        }

        public static async Task<int> AddRawMaterialAsync(RawMaterial material)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                INSERT INTO raw_materials (name, unit, material_type, cas_number, specification, is_hazardous) 
                VALUES (@name, @unit, @type, @cas, @spec, @hazardous);
                SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", material.Name ?? "");
                        cmd.Parameters.AddWithValue("@unit", material.Unit ?? "кг");
                        cmd.Parameters.AddWithValue("@type", material.MaterialType ?? "");
                        cmd.Parameters.AddWithValue("@cas", material.CasNumber ?? "");
                        cmd.Parameters.AddWithValue("@spec", material.Specification ?? "");
                        cmd.Parameters.AddWithValue("@hazardous", material.IsHazardous ?? false);

                        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add raw material error: {ex.Message}");
                return 0;
            }
        }

        #endregion
        #region Recipes (добавьте эти методы)

        public static async Task<int> AddRecipeAsync(Recipe recipe)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Вставка рецепта
                            string recipeQuery = @"
                        INSERT INTO recipes (product_id, version, status, description, created_by) 
                        VALUES (@productId, @version, @status, @description, @createdBy);
                        SELECT SCOPE_IDENTITY();";

                            int recipeId;
                            using (var cmd = new SqlCommand(recipeQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@productId", recipe.ProductId);
                                cmd.Parameters.AddWithValue("@version", recipe.Version);
                                cmd.Parameters.AddWithValue("@status", recipe.Status ?? "draft");
                                cmd.Parameters.AddWithValue("@description", recipe.Description ?? "");
                                cmd.Parameters.AddWithValue("@createdBy", recipe.CreatedBy ?? (object)DBNull.Value);

                                recipeId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            }

                            // Вставка компонентов
                            if (recipe.Components != null)
                            {
                                foreach (var comp in recipe.Components)
                                {
                                    string compQuery = @"
                                INSERT INTO recipe_components (recipe_id, material_id, quantity, unit, tolerance_pct, load_order)
                                VALUES (@recipeId, @materialId, @quantity, @unit, @tolerance, @loadOrder)";

                                    using (var cmd = new SqlCommand(compQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@recipeId", recipeId);
                                        cmd.Parameters.AddWithValue("@materialId", comp.MaterialId);
                                        cmd.Parameters.AddWithValue("@quantity", comp.Quantity);
                                        cmd.Parameters.AddWithValue("@unit", comp.Unit ?? "");
                                        cmd.Parameters.AddWithValue("@tolerance", comp.TolerancePct ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@loadOrder", comp.LoadOrder ?? (object)DBNull.Value);

                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                            }

                            transaction.Commit();
                            return recipeId;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add recipe error: {ex.Message}");
                return 0;
            }
        }

        public static async Task UpdateRecipeAsync(Recipe recipe)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Обновление рецепта
                            string recipeQuery = @"
                        UPDATE recipes 
                        SET product_id = @productId, version = @version, status = @status, description = @description
                        WHERE id = @id";

                            using (var cmd = new SqlCommand(recipeQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@id", recipe.Id);
                                cmd.Parameters.AddWithValue("@productId", recipe.ProductId);
                                cmd.Parameters.AddWithValue("@version", recipe.Version);
                                cmd.Parameters.AddWithValue("@status", recipe.Status ?? "draft");
                                cmd.Parameters.AddWithValue("@description", recipe.Description ?? "");

                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Удаление старых компонентов
                            string deleteQuery = "DELETE FROM recipe_components WHERE recipe_id = @recipeId";
                            using (var cmd = new SqlCommand(deleteQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@recipeId", recipe.Id);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Вставка новых компонентов
                            if (recipe.Components != null)
                            {
                                foreach (var comp in recipe.Components)
                                {
                                    string compQuery = @"
                                INSERT INTO recipe_components (recipe_id, material_id, quantity, unit, tolerance_pct, load_order)
                                VALUES (@recipeId, @materialId, @quantity, @unit, @tolerance, @loadOrder)";

                                    using (var cmd = new SqlCommand(compQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@recipeId", recipe.Id);
                                        cmd.Parameters.AddWithValue("@materialId", comp.MaterialId);
                                        cmd.Parameters.AddWithValue("@quantity", comp.Quantity);
                                        cmd.Parameters.AddWithValue("@unit", comp.Unit ?? "");
                                        cmd.Parameters.AddWithValue("@tolerance", comp.TolerancePct ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@loadOrder", comp.LoadOrder ?? (object)DBNull.Value);

                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update recipe error: {ex.Message}");
            }
        }

        #endregion
        #region Tech Maps (добавьте эти методы)

        public static async Task<TechMap> GetTechMapWithStepsAsync(int techMapId)
        {
            TechMap techMap = null;
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();

                    string query = "SELECT * FROM tech_maps WHERE id = @id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", techMapId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                techMap = new TechMap
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    RecipeId = reader.GetInt32(reader.GetOrdinal("recipe_id")),
                                    Name = reader["name"]?.ToString() ?? "",
                                    Version = reader.GetInt32(reader.GetOrdinal("version")),
                                    Status = reader["status"]?.ToString() ?? "",
                                    EquipmentId = reader["equipment_id"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("equipment_id")) : (int?)null,
                                    CreatedBy = reader["created_by"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("created_by")) : (int?)null,
                                    Steps = new System.Collections.Generic.List<TechStep>()
                                };
                            }
                        }
                    }

                    if (techMap != null)
                    {
                        string stepsQuery = "SELECT * FROM tech_steps WHERE tech_map_id = @mapId ORDER BY step_number";
                        using (var cmd = new SqlCommand(stepsQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@mapId", techMapId);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    techMap.Steps.Add(new TechStep
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                                        TechMapId = reader.GetInt32(reader.GetOrdinal("tech_map_id")),
                                        StepNumber = reader.GetInt32(reader.GetOrdinal("step_number")),
                                        StepType = reader["step_type"]?.ToString() ?? "",
                                        Name = reader["name"]?.ToString() ?? "",
                                        Instructions = reader["instructions"]?.ToString() ?? "",
                                        ParamName = reader["param_name"]?.ToString() ?? "",
                                        ParamTarget = reader["param_target"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("param_target")) : (decimal?)null,
                                        ParamMin = reader["param_min"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("param_min")) : (decimal?)null,
                                        ParamMax = reader["param_max"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("param_max")) : (decimal?)null,
                                        ParamUnit = reader["param_unit"]?.ToString() ?? "",
                                        DurationMin = reader["duration_min"] != DBNull.Value ? reader.GetInt32(reader.GetOrdinal("duration_min")) : (int?)null,
                                        IsMandatory = reader["is_mandatory"] != DBNull.Value && reader.GetBoolean(reader.GetOrdinal("is_mandatory"))
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get tech map with steps error: {ex.Message}");
            }
            return techMap;
        }

        public static async Task<int> AddTechMapAsync(TechMap techMap)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string query = @"
                        INSERT INTO tech_maps (recipe_id, name, version, status, equipment_id, created_by)
                        VALUES (@recipeId, @name, @version, @status, @equipmentId, @createdBy);
                        SELECT SCOPE_IDENTITY();";

                            int techMapId;
                            using (var cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@recipeId", techMap.RecipeId);
                                cmd.Parameters.AddWithValue("@name", techMap.Name ?? "");
                                cmd.Parameters.AddWithValue("@version", techMap.Version);
                                cmd.Parameters.AddWithValue("@status", techMap.Status ?? "draft");
                                cmd.Parameters.AddWithValue("@equipmentId", techMap.EquipmentId ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@createdBy", techMap.CreatedBy ?? (object)DBNull.Value);

                                techMapId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            }

                            if (techMap.Steps != null)
                            {
                                foreach (var step in techMap.Steps)
                                {
                                    string stepQuery = @"
                                INSERT INTO tech_steps (tech_map_id, step_number, step_type, name, instructions, param_name, param_target, param_min, param_max, param_unit, duration_min, is_mandatory)
                                VALUES (@mapId, @stepNumber, @stepType, @name, @instructions, @paramName, @paramTarget, @paramMin, @paramMax, @paramUnit, @duration, @isMandatory)";

                                    using (var cmd = new SqlCommand(stepQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@mapId", techMapId);
                                        cmd.Parameters.AddWithValue("@stepNumber", step.StepNumber);
                                        cmd.Parameters.AddWithValue("@stepType", step.StepType ?? "");
                                        cmd.Parameters.AddWithValue("@name", step.Name ?? "");
                                        cmd.Parameters.AddWithValue("@instructions", step.Instructions ?? "");
                                        cmd.Parameters.AddWithValue("@paramName", step.ParamName ?? "");
                                        cmd.Parameters.AddWithValue("@paramTarget", step.ParamTarget ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@paramMin", step.ParamMin ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@paramMax", step.ParamMax ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@paramUnit", step.ParamUnit ?? "");
                                        cmd.Parameters.AddWithValue("@duration", step.DurationMin ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@isMandatory", step.IsMandatory ?? false);

                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                            }

                            transaction.Commit();
                            return techMapId;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add tech map error: {ex.Message}");
                return 0;
            }
        }

        public static async Task UpdateTechMapAsync(TechMap techMap)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string query = @"
                        UPDATE tech_maps 
                        SET recipe_id = @recipeId, name = @name, version = @version, status = @status, equipment_id = @equipmentId
                        WHERE id = @id";

                            using (var cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@id", techMap.Id);
                                cmd.Parameters.AddWithValue("@recipeId", techMap.RecipeId);
                                cmd.Parameters.AddWithValue("@name", techMap.Name ?? "");
                                cmd.Parameters.AddWithValue("@version", techMap.Version);
                                cmd.Parameters.AddWithValue("@status", techMap.Status ?? "draft");
                                cmd.Parameters.AddWithValue("@equipmentId", techMap.EquipmentId ?? (object)DBNull.Value);

                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Удаление старых шагов
                            string deleteQuery = "DELETE FROM tech_steps WHERE tech_map_id = @mapId";
                            using (var cmd = new SqlCommand(deleteQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@mapId", techMap.Id);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Вставка новых шагов
                            if (techMap.Steps != null)
                            {
                                foreach (var step in techMap.Steps)
                                {
                                    string stepQuery = @"
                                INSERT INTO tech_steps (tech_map_id, step_number, step_type, name, instructions, param_name, param_target, param_min, param_max, param_unit, duration_min, is_mandatory)
                                VALUES (@mapId, @stepNumber, @stepType, @name, @instructions, @paramName, @paramTarget, @paramMin, @paramMax, @paramUnit, @duration, @isMandatory)";

                                    using (var cmd = new SqlCommand(stepQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@mapId", techMap.Id);
                                        cmd.Parameters.AddWithValue("@stepNumber", step.StepNumber);
                                        cmd.Parameters.AddWithValue("@stepType", step.StepType ?? "");
                                        cmd.Parameters.AddWithValue("@name", step.Name ?? "");
                                        cmd.Parameters.AddWithValue("@instructions", step.Instructions ?? "");
                                        cmd.Parameters.AddWithValue("@paramName", step.ParamName ?? "");
                                        cmd.Parameters.AddWithValue("@paramTarget", step.ParamTarget ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@paramMin", step.ParamMin ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@paramMax", step.ParamMax ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@paramUnit", step.ParamUnit ?? "");
                                        cmd.Parameters.AddWithValue("@duration", step.DurationMin ?? (object)DBNull.Value);
                                        cmd.Parameters.AddWithValue("@isMandatory", step.IsMandatory ?? false);

                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update tech map error: {ex.Message}");
            }
        }

        #endregion
        #region Reports

        public static async Task<DataTable> GetProductionReportAsync(DateTime startDate, DateTime endDate)
        {
            var dataTable = new DataTable();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                SELECT 
                    pb.batch_number as 'Номер партии',
                    p.name as 'Продукт',
                    pb.actual_qty as 'Количество',
                    pb.qty_unit as 'Ед.изм',
                    pb.status as 'Статус',
                    pb.finished_at as 'Дата завершения'
                FROM production_batches pb
                LEFT JOIN production_orders po ON pb.order_id = po.id
                LEFT JOIN products p ON po.product_id = p.id
                WHERE pb.finished_at BETWEEN @startDate AND @endDate
                ORDER BY pb.finished_at DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetProductionReport error: {ex.Message}");
            }
            return dataTable;
        }

        public static async Task<DataTable> GetDeviationsReportAsync(DateTime startDate, DateTime endDate)
        {
            var dataTable = new DataTable();
            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();
                    string query = @"
                SELECT 
                    pb.batch_number as 'Номер партии',
                    d.deviation_type as 'Тип отклонения',
                    d.severity as 'Важность',
                    d.description as 'Описание',
                    d.resolution_status as 'Статус',
                    d.registered_at as 'Дата регистрации'
                FROM deviations d
                LEFT JOIN production_batches pb ON d.batch_id = pb.id
                WHERE d.registered_at BETWEEN @startDate AND @endDate
                ORDER BY d.registered_at DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDeviationsReport error: {ex.Message}");
            }
            return dataTable;
        }

        #endregion

    }

}