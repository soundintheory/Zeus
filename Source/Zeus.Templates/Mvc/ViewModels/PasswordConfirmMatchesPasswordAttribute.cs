using System.ComponentModel.DataAnnotations;

namespace Zeus.Templates.Mvc.ViewModels
{
	[System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
	public class PasswordConfirmMatchesPasswordAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			var typedValue = (IConfirmPassword) value;
			if (typedValue.Password != typedValue.ConfirmPassword)
			{
				return false;
			}

			return true;
		}

		public override string FormatErrorMessage(string name)
		{
			return "Passwords must match";
		}
	}
}