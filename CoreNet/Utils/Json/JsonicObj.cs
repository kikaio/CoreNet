using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Utils.Json
{
    public class JsonicObj
    {
        protected JObject jObj;
        protected JsonicObj(JObject _jobj)
        {
            jObj = _jobj;
            var pList = GetType().GetProperties();
            foreach (var pinfo in pList)
            {
                var jToken = jObj.GetValue(pinfo.Name.ToUpper());
                if (jToken != null)
                    SetPropertyFromJToken(pinfo, jToken);
                else
                    ;
            }
        }
        //JToken의 값은 무조건 string으로 변환 후 다시 형변화 한다.=> 안전성이냐 최적화냐
        protected void SetPropertyFromJToken(PropertyInfo _pInfo, JToken _jToken)
        {
            var pType = _pInfo.PropertyType;
            var pCode = Type.GetTypeCode(pType);
            var strVal = _jToken.ToString();
            switch (pCode)
            {
                case TypeCode.Boolean:
                    if (strVal == "1")
                        _pInfo.SetValue(this, true);
                    else if (strVal == "0")
                        _pInfo.SetValue(this, false);
                    else if (strVal.ToLower() == "true")
                        _pInfo.SetValue(this, true);
                    else
                        _pInfo.SetValue(this, false);
                    break;
                case TypeCode.Int16:
                    {
                        var val = default(Int16);
                        if (Int16.TryParse(strVal, out val))
                            _pInfo.SetValue(this, val);
                        else
                            ;
                    }
                    break;
                case TypeCode.Int32:
                    {
                        var val = default(Int32);
                        if (Int32.TryParse(strVal, out val))
                            _pInfo.SetValue(this, val);
                        else
                            ;
                    }
                    break;
                case TypeCode.Int64:
                    {
                        var val = default(Int64);
                        if (Int64.TryParse(strVal, out val))
                            _pInfo.SetValue(this, val);
                        else
                            ;
                    }
                    break;
                case TypeCode.Double:
                    {
                        var val = default(Double);
                        if (Double.TryParse(strVal, out val))
                            _pInfo.SetValue(this, val);
                        else
                            ;
                    }
                    break;
                case TypeCode.String:
                    {
                        _pInfo.SetValue(this, strVal);
                    }
                    break;
                //JSON읽어올 때 해당 타입들 안쓸거임.
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                    break;
                default:
                    break;
            }
        }
    }

}
