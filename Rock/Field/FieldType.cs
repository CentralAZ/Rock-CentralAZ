﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field
{
    /// <summary>
    /// Abstract class that all custom field types should inherit from
    /// </summary>
    [Serializable]
    public abstract class FieldType : IFieldType
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        public FieldType()
        {
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public virtual List<string> ConfigurationKeys()
        {
            return new List<string>();
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public virtual List<Control> ConfigurationControls()
        {
            return new List<Control>();
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public virtual Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            return new Dictionary<string, ConfigurationValue>();
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public virtual void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Gets the align value that should be used when displaying value
        /// </summary>
        public virtual HorizontalAlign AlignValue
        {
            get { return HorizontalAlign.Left; }
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public virtual string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( condensed )
            {
                return value.Truncate( 100 );
            }

            return value;
        }

        /// <summary>
        /// Formats the value.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public virtual string FormatValue( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return FormatValue( parentControl, value, configurationValues, condensed );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public virtual string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return FormatValue( parentControl, value, configurationValues, condensed );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public virtual string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return FormatValue( parentControl, entityTypeId, entityId, value, configurationValues, condensed );
        }

        /// <summary>
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public virtual object ValueAsFieldType( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // by default, get the field type's value
            return value;
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public virtual object SortValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // by default, get the formatted condensed value that would be displayed to the user
            return FormatValue( parentControl, value, configurationValues, true );
        }

        /// <summary>
        /// Setting to determine whether the value from this control is sensitive.  This is used for determining
        /// whether or not the value of this attribute is logged when changed.
        /// </summary>
        /// <returns>
        ///   <c>false</c> By default, any field is not sensitive.
        /// </returns>
        public virtual bool IsSensitive()
        {
            return false;
        }
        #endregion

        #region Edit Control

        /// <summary>
        /// Gets a value indicating whether this field has a control to configure the default value
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has default control; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasDefaultControl => true;

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public virtual Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new Rock.Web.UI.Controls.RockTextBox { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public virtual string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is TextBox )
            {
                return ( ( TextBox ) control ).Text;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public virtual void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is TextBox )
            {
                ( ( TextBox ) control ).Text = value;
            }
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is valid; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsValid( string value, bool required, out string message )
        {
            if ( required && string.IsNullOrWhiteSpace( value ) )
            {
                message = "value is required.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public virtual Control FilterControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            HtmlGenericControl row = new HtmlGenericControl( "div" );
            row.ID = id;
            row.AddCssClass( "row" );
            row.AddCssClass( "field-criteria" );

            var compareControl = FilterCompareControl( configurationValues, id, required, filterMode );
            var valueControl = FilterValueControl( configurationValues, id, required, filterMode );

            string col1Class = string.Empty;
            string col2Class = "col-md-12";

            if ( compareControl != null )
            {
                HtmlGenericControl col1 = new HtmlGenericControl( "div" );
                col1.ID = string.Format( "{0}_col1", id );
                row.Controls.Add( col1 );
                if ( !compareControl.Visible )
                {
                    col1Class = string.Empty;
                    col2Class = "col-md-12";
                }
                else if ( compareControl is Label )
                {
                    col1Class = "col-md-2";
                    col2Class = "col-md-10";
                }
                else
                {
                    col1Class = "col-md-4";
                    col2Class = "col-md-8";
                }

                col1.AddCssClass( col1Class );
                col1.Controls.Add( compareControl );
            }

            HtmlGenericControl col2 = new HtmlGenericControl( "div" );
            col2.ID = string.Format( "{0}_col2", id );
            row.Controls.Add( col2 );
            col2.AddCssClass( col2Class );
            if ( valueControl != null )
            {
                col2.Controls.Add( valueControl );
            }

            return row;
        }

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type using a FilterMode of AdvancedFilter
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        public virtual Control FilterControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required )
        {
            return FilterControl( configurationValues, id, required, FilterMode.AdvancedFilter );
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public virtual bool HasFilterControl()
        {
            return true;
        }

        /// <summary>
        /// Gets the filter compare control with the specified FilterMode
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public virtual Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            RockDropDownList ddlCompare = ComparisonHelper.ComparisonControl( FilterComparisonType, required );

            if ( filterMode == FilterMode.SimpleFilter && (
                FilterComparisonType == ComparisonHelper.BinaryFilterComparisonTypes ||
                FilterComparisonType == ComparisonHelper.StringFilterComparisonTypes ||
                FilterComparisonType == ComparisonHelper.ContainsFilterComparisonTypes ) )
            {
                // hide the compare control for SimpleFilter mode if it is a string, list, or binary comparison type
                ddlCompare.Visible = false;
            }

            ddlCompare.ID = string.Format( "{0}_ddlCompare", id );
            ddlCompare.AddCssClass( "js-filter-compare" );
            return ddlCompare;
        }

        /// <summary>
        /// Returns the ComparisonType options that the field supports
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public virtual ComparisonType FilterComparisonType
        {
            get { return ComparisonHelper.BinaryFilterComparisonTypes; }
        }

        /// <summary>
        /// Gets the filter value control with the specified FilterMode
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public virtual Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var control = EditControl( configurationValues, id );
            control.ID = string.Format( "{0}_ctlCompareValue", id );
            if ( control is WebControl )
            {
                ( ( WebControl ) control ).AddCssClass( "js-filter-control" );
            }

            return control;
        }

        /// <summary>
        /// Gets the filter value.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public virtual List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, FilterMode filterMode )
        {
            var values = new List<string>();

            if ( filterControl != null )
            {
                try
                {
                    string compare = GetFilterCompareValue( filterControl.Controls[0].Controls[0], filterMode );
                    if ( compare != null )
                    {
                        values.Add( compare );
                    }

                    string value = GetFilterValueValue( filterControl.Controls[1].Controls[0], configurationValues );
                    if ( value != null )
                    {
                        values.Add( value );
                    }
                }
                catch
                {
                    // intentionally ignore error
                }
            }

            return values;
        }

        /// <summary>
        /// Gets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public virtual string GetFilterCompareValue( Control control, FilterMode filterMode )
        {
            var ddlCompare = control as RockDropDownList;
            if ( ddlCompare != null )
            {
                bool filterValueControlVisible = true;
                var filterField = control.FirstParentControlOfType<FilterField>();
                if ( filterField != null && filterField.HideFilterCriteria )
                {
                    filterValueControlVisible = false;
                }

                // if the CompareControl is hidden, but the ValueControl is visible, pick the appropriate ComparisonType
                if ( !ddlCompare.Visible && filterValueControlVisible )
                {
                    if ( filterMode == FilterMode.SimpleFilter )
                    {
                        // in FilterMode.SimpleFilter...
                        if ( FilterComparisonType == ComparisonHelper.BinaryFilterComparisonTypes )
                        {
                            // ...if the compare only support EqualTo/NotEqual to, return EqualTo
                            return ComparisonType.EqualTo.ConvertToInt().ToString();
                        }

                        if ( FilterComparisonType == ComparisonHelper.StringFilterComparisonTypes ||
                            FilterComparisonType == ComparisonHelper.ContainsFilterComparisonTypes )
                        {
                            // ... if the compare is the string or list type comparison, return Contains
                            return ComparisonType.Contains.ConvertToInt().ToString();
                        }
                    }
                }

                return ddlCompare.SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the equal to compare value (types that don't support an equalto comparison (i.e. singleselect) should return null
        /// </summary>
        /// <returns></returns>
        public virtual string GetEqualToCompareValue()
        {
            return ComparisonType.EqualTo.ConvertToInt().ToString();
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public virtual string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return GetEditValue( control, configurationValues );
        }

        /// <summary>
        /// Sets the filter value.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        public virtual void SetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            if ( filterControl != null &&
                filterControl.Controls != null &&
                filterControl.Controls.Count != 0 &&
                filterControl.Controls[0].Controls != null &&
                filterControl.Controls[0].Controls.Count != 0  &&
                filterValues != null )
            {
                try
                {
                    SetFilterCompareValue( filterControl.Controls[0].Controls[0], filterValues.Count > 0 ? filterValues[0] : string.Empty );

                    if ( filterControl.Controls.Count > 1 &&
                        filterControl.Controls[1].Controls != null &&
                        filterControl.Controls[1].Controls.Count != 0 &&
                        filterValues != null )
                    {
                        string value = filterValues.Count > 1 ? filterValues[1] : filterValues.Count > 0 ? filterValues[0] : string.Empty;
                        SetFilterValueValue( filterControl.Controls[1].Controls[0], configurationValues, value );
                    }
                }
                catch
                {
                    // intentionally ignore error
                }
            }
        }

        /// <summary>
        /// Sets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value.</param>
        public virtual void SetFilterCompareValue( Control control, string value )
        {
            var ddlCompare = control as RockDropDownList;
            if ( ddlCompare != null )
            {
                ddlCompare.SetValue( value );
            }
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public virtual void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            SetEditValue( control, configurationValues, value );
        }

        /// <summary>
        /// Formats the filter values.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <returns></returns>
        public virtual string FormatFilterValues( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            if ( filterValues != null && filterValues.Any() )
            {
                if ( filterValues.Count == 1 )
                {
                    // If just one value, then it is likely just a value
                    string filterValue = FormatFilterValueValue( configurationValues, filterValues[0] );
                    if ( !string.IsNullOrWhiteSpace( filterValue ) )
                    {
                        return "is " + filterValue;
                    }
                }
                else if ( filterValues.Count >= 2 )
                {
                    // If two more values, then it is a comparison and a value
                    string comparisonValue = filterValues[0];
                    if ( comparisonValue != "0" )
                    {
                        ComparisonType comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
                        if ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank )
                        {
                            return comparisonType.ConvertToString();
                        }
                        else
                        {
                            var filterValueValue = FormatFilterValueValue( configurationValues, filterValues[1] );
                            if ( string.IsNullOrEmpty( filterValueValue ) )
                            {
                                return string.Format( "{0} ''", comparisonType.ConvertToString() );
                            }
                            else
                            {
                                return string.Format( "{0} {1}", comparisonType.ConvertToString(), filterValueValue );
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            string formattedValue = FormatValue( null, value, configurationValues, false );
            if ( !string.IsNullOrWhiteSpace( formattedValue ) )
            {
                return string.Format( "'{0}'", formattedValue );
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the filter format script.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        /// <remarks>
        /// This script must set a javascript variable named 'result' to a friendly string indicating value of filter controls
        /// a '$selectedContent' should be used to limit script to currently selected filter fields
        /// </remarks>
        public virtual string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title )
        {
            string titleJs = System.Web.HttpUtility.JavaScriptStringEncode( title );
            return string.Format( "return Rock.reporting.formatFilterDefault('{0}', $selectedContent);", titleJs );
        }

        /// <summary>
        /// Gets a filter expression for an entity property value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public virtual Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            return Rock.Utility.ExpressionHelper.PropertyFilterExpression( filterValues, parameterExpression, propertyName, propertyType );
        }

        /// <summary>
        /// Converts the type of the value to property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="isNullableType">if set to <c>true</c> [is nullable type].</param>
        /// <returns></returns>
        public virtual object ConvertValueToPropertyType( string value, Type propertyType, bool isNullableType )
        {
            return Rock.Utility.ExpressionHelper.ConvertValueToPropertyType( value, propertyType, isNullableType );
        }

        /// <summary>
        /// Gets a filter expression to be used as part of a AttributeValue Query or EntityAttributeQueryExpression
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public virtual Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            if ( filterValues.Count >= 2 )
            {
                ComparisonType? comparisonType = filterValues[0].ConvertToEnumOrNull<ComparisonType>();
                if ( comparisonType.HasValue )
                {
                    string compareToValue = filterValues[1];
                    bool valueNotNeeded = ( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType );

                    if ( valueNotNeeded || !string.IsNullOrWhiteSpace( compareToValue ) )
                    {
                        MemberExpression propertyExpression = Expression.Property( parameterExpression, this.AttributeValueFieldName );
                        return ComparisonHelper.ComparisonExpression( comparisonType.Value, propertyExpression, AttributeConstantExpression( compareToValue ) );
                    }
                }
                else
                {
                    // No comparison type specified, so return NoAttributeFilterExpression ( which means don't filter )
                    return new NoAttributeFilterExpression();
                }
            }

            // return null if there isn't an additional expression that will help narrow down which AttributeValue records to include
            return null;
        }

        /// <summary>
        /// Gets the Attribute Query Expression to be used with an Entity Query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="qry">The qry.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="serviceInstance"></param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public virtual IQueryable<T> ApplyAttributeQueryFilter<T>( IQueryable<T> qry, Control filterControl, Rock.Web.Cache.AttributeCache attribute, IService serviceInstance, Rock.Reporting.FilterMode filterMode ) where T : Rock.Data.Entity<T>, new()
        {
            if ( filterControl == null )
            {
                return qry;
            }

            var filterValues = GetFilterValues( filterControl, attribute.QualifierValues, filterMode );
            var entityFields = EntityHelper.GetEntityFields( typeof( T ) );
            var entityField = entityFields.Where( a => a.FieldKind == FieldKind.Attribute && a.AttributeGuid == attribute.Guid ).FirstOrDefault();

            if ( entityField == null )
            {
                entityField = EntityHelper.GetEntityFieldForAttribute( attribute, false );
            }

            var parameterExpression = serviceInstance.ParameterExpression;
            var attributeExpression = Rock.Utility.ExpressionHelper.GetAttributeExpression( serviceInstance, parameterExpression, entityField, filterValues );
            qry = qry.Where( parameterExpression, attributeExpression );

            return qry;
        }

        /// <summary>
        /// Determines whether the filter is an 'Equal To' comparison and the filtered value is equal to the specified value.
        /// </summary>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is equal to value] [the specified filter values]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsEqualToValue( List<string> filterValues, string value )
        {
            if ( filterValues == null || filterValues.Count != 2 )
            {
                return false;
            }

            ComparisonType? filterComparisonType = filterValues[0].ConvertToEnumOrNull<ComparisonType>();
            ComparisonType? equalToCompareValue = GetEqualToCompareValue().ConvertToEnumOrNull<ComparisonType>();

            if ( filterComparisonType != equalToCompareValue )
            {
                return false;
            }

            return filterValues[1] == value;
        }

        /// <summary>
        /// Determines whether the filter's comparison type and filter compare value(s) evaluates to true for the specified value
        /// </summary>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is compared to value] [the specified filter values]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsComparedToValue( List<string> filterValues, string value )
        {
            return IsEqualToValue( filterValues, value );
        }

        /// <summary>
        /// Gets the name of the attribute value field that should be bound to (Value, ValueAsDateTime, ValueAsBoolean, or ValueAsNumeric)
        /// </summary>
        /// <value>
        /// The name of the attribute value field.
        /// </value>
        public virtual string AttributeValueFieldName
        {
            get
            {
                return "Value";
            }
        }

        /// <summary>
        /// Attributes the constant expression.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual ConstantExpression AttributeConstantExpression( string value )
        {
            return Expression.Constant( value, this.AttributeValueFieldType );
        }

        /// <summary>
        /// Gets the type of the attribute value field.
        /// </summary>
        /// <value>
        /// The type of the attribute value field.
        /// </value>
        public virtual Type AttributeValueFieldType
        {
            get
            {
                return typeof( string );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when [qualifier updated].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        public void OnQualifierUpdated( object sender, EventArgs e )
        {
            if ( QualifierUpdated != null )
            {
                QualifierUpdated( sender, e );
            }
        }

        /// <summary>
        /// Occurs when [qualifier updated].
        /// </summary>
        public event EventHandler QualifierUpdated;

        #endregion
    }
}