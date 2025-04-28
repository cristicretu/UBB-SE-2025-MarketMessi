using MarketMinds.Shared.Models;
using MarketMinds.Repositories.ProductConditionRepository;
using System.Collections.Generic;

namespace MarketMinds.Test.Services.ProductConditionService
{
    internal class ProductConditionRepositoryMock : IProductConditionRepository
    {
        public List<Condition> Conditions { get; set; } = new List<Condition>();
        private int currentIndex = 0;

        public ProductConditionRepositoryMock() 
        { 
            Conditions = new List<Condition>();
        }

        Condition IProductConditionRepository.CreateProductCondition(string displayTitle, string description)
        {
            var newCondition = new Condition(
                id: ++currentIndex,
                displayTitle: displayTitle,
                description: description
            );

            Conditions.Add(newCondition);
            return newCondition;
        }

        void IProductConditionRepository.DeleteProductCondition(string displayTitle)
        {
            Conditions.RemoveAll(cond => cond.DisplayTitle == displayTitle);
        }

        List<Condition> IProductConditionRepository.GetAllProductConditions()
        {
            return Conditions;
        }
    }
}
