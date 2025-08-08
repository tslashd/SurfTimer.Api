using SurfTimer.Shared.Types;
using Dapper;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace CS2_Surf_NET_API.Shared.TypeHandlers
{
    public class ReplayFramesStringHandler : SqlMapper.TypeHandler<ReplayFramesString>
    {
        public override ReplayFramesString Parse(object value)
        {
            if (value is byte[] bytes)
            {
                try
                {
                    var str = Encoding.UTF8.GetString(bytes);

                    if (IsBase64String(str))
                        return new ReplayFramesString(str);

                    return new ReplayFramesString(Convert.ToBase64String(bytes));
                }
                catch
                {
                    return new ReplayFramesString(Convert.ToBase64String(bytes));
                }
            }

            return new ReplayFramesString(string.Empty);
        }

        //public override void SetValue(IDbDataParameter parameter, ReplayFramesString? value)
        //{
        //    parameter.Value = Convert.FromBase64String(value?.Value);
        //}

        public override void SetValue(IDbDataParameter parameter, ReplayFramesString? value)
        {
            parameter.Value = value is null
                ? DBNull.Value
                : Convert.FromBase64String(value.Value);
        }

        private static bool IsBase64String(string str)
        {
            return str.Length % 4 == 0 && Regex.IsMatch(str, @"^[a-zA-Z0-9\+/]*={0,2}$");
        }
    }
}
