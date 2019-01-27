/***************************************************************************/
// Copyright 2013-2019 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Xunit;

namespace Bix.Core
{
    public class ModelExtensionsTest
    {
        #region IsAggregateRootModelType Tests

        [Fact]
        public void IsAggregateRootModelType_true_for_IAggregateRoot_ModelBase()
        {
            var mock = new Mock<ModelBase>().As<IAggregateRoot>();
            Assert.True(mock.Object.GetType().IsAggregateRootModelType());
        }

        [Fact]
        public void IsAggregateRootModelType_false_for_IAggregateRoot_non_ModelBase()
        {
            var mock = new Mock<object>().As<IAggregateRoot>();
            Assert.False(mock.Object.GetType().IsAggregateRootModelType());
        }

        [Fact]
        public void IsAggregateRootModelType_false_for_non_IAggregateRoot_ModelBase()
        {
            var mock = new Mock<ModelBase>();
            Assert.False(mock.Object.GetType().IsAggregateRootModelType());
        }

        [Fact]
        public void IsAggregateRootModelType_false_for_non_IAggregateRoot_non_ModelBase()
        {
            var mock = new Mock<object>();
            Assert.False(mock.Object.GetType().IsAggregateRootModelType());
        }

        #endregion

        #region IsNonAggregateRootModelType Tests

        [Fact]
        public void IsNonAggregateRootModelType_false_for_IAggregateRoot_ModelBase()
        {
            var mock = new Mock<ModelBase>().As<IAggregateRoot>();
            Assert.False(mock.Object.GetType().IsNonAggregateRootModelType());
        }

        [Fact]
        public void IsNonAggregateRootModelType_false_for_IAggregateRoot_non_ModelBase()
        {
            var mock = new Mock<object>().As<IAggregateRoot>();
            Assert.False(mock.Object.GetType().IsNonAggregateRootModelType());
        }

        [Fact]
        public void IsNonAggregateRootModelType_true_for_non_IAggregateRoot_ModelBase()
        {
            var mock = new Mock<ModelBase>();
            Assert.True(mock.Object.GetType().IsNonAggregateRootModelType());
        }

        [Fact]
        public void IsNonAggregateRootModelType_false_for_non_IAggregateRoot_non_ModelBase()
        {
            var mock = new Mock<object>();
            Assert.False(mock.Object.GetType().IsNonAggregateRootModelType());
        }

        #endregion

        #region IsChildModelProperty Tests

        [Fact]
        public void IsChildModelProperty_false_for_read_write_property_that_is_an_aggregate_root()
        {
            var mock = new Mock<ModelBase>().As<IAggregateRoot>();
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == true && p.CanWrite == true && p.PropertyType == mock.Object.GetType());
            Assert.False(property.IsChildModelProperty());
        }

        [Fact]
        public void IsChildModelProperty_true_for_read_write_property_that_is_a_non_aggregate_root_model()
        {
            var mock = new Mock<ModelBase>();
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == true && p.CanWrite == true && p.PropertyType == mock.Object.GetType());
            Assert.True(property.IsChildModelProperty());
        }

        [Fact]
        public void IsChildModelProperty_false_for_read_only_child_model()
        {
            var mock = new Mock<ModelBase>();
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == true && p.CanWrite == false && p.PropertyType == mock.Object.GetType());
            Assert.False(property.IsChildModelProperty());
        }

        [Fact]
        public void IsChildModelProperty_false_for_write_only_child_model()
        {
            var mock = new Mock<ModelBase>();
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == false && p.CanWrite == true && p.PropertyType == mock.Object.GetType());
            Assert.False(property.IsChildModelProperty());
        }

        #endregion

        #region IsChildModelCollectionProperty Tests

        [Fact]
        public void IsChildModelCollectionProperty_true_for_read_write_CollectionT_property_that_is_a_non_aggregate_root_model()
        {
            var mock = new Mock<ModelBase>();
            var type = typeof(Collection<>).MakeGenericType(mock.Object.GetType());
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == true && p.CanWrite == true && p.PropertyType == type);

            Assert.True(property.IsChildModelCollectionProperty(out var childModelType));
            Assert.Equal(mock.Object.GetType(), childModelType);
        }

        [Fact]
        public void IsChildModelCollectionProperty_true_for_read_write_array_property_that_is_a_non_aggregate_root_model()
        {
            var mock = new Mock<ModelBase>();
            var type = mock.Object.GetType().MakeArrayType(1);
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == true && p.CanWrite == true && p.PropertyType == type);

            Assert.True(property.IsChildModelCollectionProperty(out var childModelType));
            Assert.Equal(mock.Object.GetType(), childModelType);
        }

        [Fact]
        public void IsChildModelCollectionProperty_true_for_read_write_ICollectionT_property_that_is_a_non_aggregate_root_model()
        {
            var mock = new Mock<ModelBase>();
            var type = typeof(ICollection<>).MakeGenericType(mock.Object.GetType());
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == true && p.CanWrite == true && p.PropertyType == type);

            Assert.True(property.IsChildModelCollectionProperty(out var childModelType));
            Assert.Equal(mock.Object.GetType(), childModelType);
        }

        [Fact]
        public void IsChildModelCollectionProperty_true_for_read_only_ListT_property_that_is_a_non_aggregate_root_model()
        {
            var mock = new Mock<ModelBase>();
            var type = typeof(List<>).MakeGenericType(mock.Object.GetType());
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == true && p.CanWrite == false && p.PropertyType == type);

            Assert.True(property.IsChildModelCollectionProperty(out var childModelType));
            Assert.Equal(mock.Object.GetType(), childModelType);
        }

        [Fact]
        public void IsChildModelCollectionProperty_false_for_write_only_ListT_property_that_is_a_non_aggregate_root_model()
        {
            var mock = new Mock<ModelBase>();
            var type = typeof(List<>).MakeGenericType(mock.Object.GetType());
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == false && p.CanWrite == true && p.PropertyType == type);

            Assert.False(property.IsChildModelCollectionProperty(out var childModelType));
            Assert.Null(childModelType);
        }

        [Fact]
        public void IsChildModelCollectionProperty_false_for_read_write_ListT_property_that_is_an_aggregate_root_model()
        {
            var mock = new Mock<ModelBase>().As<IAggregateRoot>();
            var type = typeof(List<>).MakeGenericType(mock.Object.GetType());
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == true && p.CanWrite == true && p.PropertyType == type);

            Assert.False(property.IsChildModelCollectionProperty(out var childModelType));
            Assert.Null(childModelType);
        }

        [Fact]
        public void IsChildModelCollectionProperty_false_for_read_write_ListT_property_that_is_not_a_model()
        {
            var mock = new Mock<object>();
            var type = typeof(List<>).MakeGenericType(mock.Object.GetType());
            var property = Mock.Of<PropertyInfo>(p => p.CanRead == true && p.CanWrite == true && p.PropertyType == type);

            Assert.False(property.IsChildModelCollectionProperty(out var childModelType));
            Assert.Null(childModelType);
        }

        #endregion
    }
}
