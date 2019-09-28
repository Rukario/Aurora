﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AuroraUI.Controls.InputField {

    /// <summary>
    /// Represents a control that has a particular (variable) type and presents a relevant control to the user.
    /// </summary>
    public sealed class InputField<TValue> : ComponentBase {

        [Parameter] public TValue Value { get; set; }
        [Parameter] public EventCallback<TValue> ValueChanged { get; set; }


        protected override void BuildRenderTree(RenderTreeBuilder builder) {

            if (GetControlFor(typeof(TValue)) is { } control) {
                // If a control exists, create markup for this control, adding the relevant Value and ValueChanged attributes
                builder.OpenComponent(0, control);
                builder.AddAttribute(1, "Value", Value);
                builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<TValue>(this, OnInputValueChanged));
                builder.CloseComponent();

            } else if (typeof(TValue).IsEnum) {
                // For enums, if a control exists for this specific enum use that (in the above branch), else use the default enum combo
                builder.OpenComponent<EnumCombobox<TValue>>(0);
                builder.AddAttribute(1, "SelectedItem", Value);
                builder.AddAttribute(2, "SelectedItemChanged", EventCallback.Factory.Create<TValue>(this, OnInputValueChanged));
                builder.CloseComponent();

            } else
                // If no control exists for this datatype, show a warning message
                builder.AddMarkupContent(0, $"<div class='editor-type-error'>Editor for the data type '{typeof(TValue).Name}' is unavailable.</div>");
        }

        private void OnInputValueChanged(TValue newValue) {
            Value = newValue;
            ValueChanged.InvokeAsync(Value);
        }

        /// <summary>Gets the editor type that will be used for the given data type.</summary>
        private static Type GetControlFor(Type dataType, string specialName = "")
            => availableControls.ContainsKey((dataType, specialName.ToLower())) ? availableControls[(dataType, specialName)] // Try find a matching type and special name
             : availableControls.ContainsKey((dataType, "")) ? availableControls[(dataType, specialName)] // If not, try find the type with no special name
             : null; // Found no matching control

        // A list of all available inner editor controls
        private static Dictionary<(Type dataType, string specialName), Type> availableControls =
            Aurora.Utils.TypeUtils.GetTypesWithCustomAttribute<InputFieldControlAttribute>()
                .Where(x => typeof(ComponentBase).IsAssignableFrom(x.Key))
                .ToDictionary(x => (x.Value.Type, x.Value.SpecialName?.ToLower() ?? ""), x => x.Key);
    }
}