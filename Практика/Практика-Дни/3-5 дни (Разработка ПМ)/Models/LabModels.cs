// Models/LabModels.cs
using System;
using System.Collections.Generic;

namespace WpfApp1.Models
{

    public class LabTestHistory
    {
        public int Id { get; set; }
        public int TestId { get; set; }                 // ID испытания
        public string Action { get; set; }              // Действие: created, started, result_added, completed, etc
        public string Description { get; set; }         // Описание
        public int? UserId { get; set; }                // ID пользователя
        public DateTime CreatedAt { get; set; }         // Дата действия
        public string OldValue { get; set; }            // Старое значение
        public string NewValue { get; set; }            // Новое значение

        public virtual LabTest LabTest { get; set; }
        public virtual User User { get; set; }
    }

    /// <summary>
    /// Шаблон протокола испытаний
    /// </summary>
    public class TestProtocolTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }                // Название шаблона
        public string ObjectType { get; set; }          // Тип объекта
        public int? MaterialTypeId { get; set; }        // Тип материала (для сырья)
        public int? ProductTypeId { get; set; }         // Тип продукции
        public bool IsActive { get; set; }              // Активен
        public string Description { get; set; }         // Описание

        public virtual List<TestProtocolParameter> Parameters { get; set; }
    }

    /// <summary>
    /// Параметры шаблона протокола
    /// </summary>
    public class TestProtocolParameter
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }             // ID шаблона
        public string ParameterName { get; set; }       // Наименование параметра
        public decimal? DefaultMinValue { get; set; }   // Мин. значение по умолчанию
        public decimal? DefaultMaxValue { get; set; }   // Макс. значение по умолчанию
        public string Unit { get; set; }                // Единица измерения
        public int SortOrder { get; set; }              // Порядок сортировки
        public bool IsRequired { get; set; }            // Обязательный
    }
}