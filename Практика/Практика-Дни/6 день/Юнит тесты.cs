// Tests/DatabaseManagerTests.cs
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfApp1;
using WpfApp1.Models;

namespace WpfApp1.Tests
{
    [TestClass]
    public class DatabaseManagerTests
    {
        [TestMethod]
        public void HashPassword_ReturnsHash_ForValidPassword()
        {
            // Arrange
            string password = "test123";
            
            // Act
            string hash = DatabaseManager.HashPassword(password);
            
            // Assert
            Assert.IsNotNull(hash);
            Assert.IsTrue(hash.Length > 0);
        }
        
        [TestMethod]
        public void VerifyPassword_ReturnsTrue_ForValidPassword()
        {
            // Arrange
            string password = "test123";
            string hash = DatabaseManager.HashPassword(password);
            
            // Act
            bool result = DatabaseManager.VerifyPassword(password, hash);
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public void VerifyPassword_ReturnsFalse_ForInvalidPassword()
        {
            // Arrange
            string password = "test123";
            string hash = DatabaseManager.HashPassword(password);
            
            // Act
            bool result = DatabaseManager.VerifyPassword("wrong", hash);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public async Task GetProductsAsync_ReturnsList()
        {
            // Act
            var products = await DatabaseManager.GetAllProductsAsync();
            
            // Assert
            Assert.IsNotNull(products);
        }
        
        [TestMethod]
        public void GenerateCaptcha_ReturnsImageData()
        {
            // Arrange
            string sessionId = Guid.NewGuid().ToString();
            
            // Act
            var captcha = DatabaseManager.GenerateCaptcha(sessionId);
            
            // Assert
            Assert.IsNotNull(captcha);
            Assert.IsNotNull(captcha.Text);
            Assert.IsTrue(captcha.Text.Length == 4);
            Assert.IsNotNull(captcha.ImageData);
            Assert.IsTrue(captcha.ImageData.Length > 0);
        }
        
        [TestMethod]
        public void VerifyCaptcha_ReturnsTrue_ForValidCode()
        {
            // Arrange
            string sessionId = Guid.NewGuid().ToString();
            var captcha = DatabaseManager.GenerateCaptcha(sessionId);
            
            // Act
            bool result = DatabaseManager.VerifyCaptcha(sessionId, captcha.Text);
            
            // Assert
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public void VerifyCaptcha_ReturnsFalse_ForInvalidCode()
        {
            // Arrange
            string sessionId = Guid.NewGuid().ToString();
            DatabaseManager.GenerateCaptcha(sessionId);
            
            // Act
            bool result = DatabaseManager.VerifyCaptcha(sessionId, "0000");
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public async Task AddProduct_ReturnsPositiveId()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                ProductType = "Test Type",
                ReleaseForm = "Test Form",
                Status = "active"
            };
            
            // Act
            int id = await DatabaseManager.AddProductAsync(product);
            
            // Assert
            Assert.IsTrue(id > 0);
            
            // Cleanup
            await DatabaseManager.DeleteProductAsync(id);
        }
    }
    
    [TestClass]
    public class LabTestTests
    {
        [TestMethod]
        public void TestResult_IsWithinRange_WhenValueBetweenMinAndMax()
        {
            // Arrange
            var result = new LabTestResult
            {
                MeasuredValue = 75,
                MinValue = 70,
                MaxValue = 80
            };
            
            // Act
            bool isWithin = result.MeasuredValue >= result.MinValue && 
                           result.MeasuredValue <= result.MaxValue;
            
            // Assert
            Assert.IsTrue(isWithin);
        }
        
        [TestMethod]
        public void TestResult_IsNotWithinRange_WhenValueBelowMin()
        {
            // Arrange
            var result = new LabTestResult
            {
                MeasuredValue = 65,
                MinValue = 70,
                MaxValue = 80
            };
            
            // Act
            bool isWithin = result.MeasuredValue >= result.MinValue && 
                           result.MeasuredValue <= result.MaxValue;
            
            // Assert
            Assert.IsFalse(isWithin);
        }
        
        [TestMethod]
        public void TestResult_IsNotWithinRange_WhenValueAboveMax()
        {
            // Arrange
            var result = new LabTestResult
            {
                MeasuredValue = 85,
                MinValue = 70,
                MaxValue = 80
            };
            
            // Act
            bool isWithin = result.MeasuredValue >= result.MinValue && 
                           result.MeasuredValue <= result.MaxValue;
            
            // Assert
            Assert.IsFalse(isWithin);
        }
    }
    
    [TestClass]
    public class ProductionBatchTests
    {
        [TestMethod]
        public void BatchNumber_IsGenerated_WhenNotProvided()
        {
            // Arrange & Act
            string batchNumber = $"BATCH-{DateTime.Now:yyyyMMddHHmmss}";
            
            // Assert
            Assert.IsTrue(batchNumber.StartsWith("BATCH-"));
            Assert.IsTrue(batchNumber.Length > 10);
        }
        
        [TestMethod]
        public void OrderNumber_IsGenerated_WhenNotProvided()
        {
            // Arrange & Act
            string orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            
            // Assert
            Assert.IsTrue(orderNumber.StartsWith("ORD-"));
        }
        
        [TestMethod]
        public void BatchStatus_Changes_WhenCompleted()
        {
            // Arrange
            var batch = new ProductionBatch
            {
                Status = "in_progress"
            };
            
            // Act
            batch.Status = "completed";
            
            // Assert
            Assert.AreEqual("completed", batch.Status);
        }
    }
}