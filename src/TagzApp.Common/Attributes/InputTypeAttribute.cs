using System.Diagnostics.CodeAnalysis;

namespace TagzApp.Common.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Class | AttributeTargets.Method)]
public class InputTypeAttribute : Attribute
{
	public static readonly InputTypeAttribute Default = new();

	public InputTypeAttribute() : this(string.Empty)
	{
	}

	public InputTypeAttribute(string inputType)
	{
		InputTypeValue = inputType;
	}

	public virtual string InputType => InputTypeValue;

	protected string InputTypeValue { get; set; }

	public override bool Equals([NotNullWhen(true)] object? obj) =>
			obj is InputTypeAttribute other && other.InputType == InputType;

	public override int GetHashCode() => InputType?.GetHashCode() ?? 0;

	public override bool IsDefaultAttribute() => Equals(Default);
}
