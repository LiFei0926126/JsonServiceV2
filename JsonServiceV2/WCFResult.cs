using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JsonService
{
    public class WCFResult
    {
        #region 私有变量
        protected bool m_Result;
        protected string m_Message;
        protected object m_Data;
        #endregion

        #region 构造函数
        public WCFResult(bool result)
        {
            m_Result = result;
        }
        public WCFResult(bool result, string strMessage)
        {
            m_Result = result;
            m_Message = strMessage;
        }
        public WCFResult(bool result, string strMessage, object objData)
        {
            m_Result = result;
            m_Message = strMessage;
            m_Data = objData;
        }
        #endregion

        #region 属性
        public bool Result
        {
            get
            {
                return m_Result;
            }
            set
            {
                m_Result = value;
            }
        }
        public string Message
        {
            get
            {
                return m_Message;
            }
            set
            {
                m_Message = value;
            }
        }
        public object Data
        {
            get
            {
                return m_Data;
            }
            set
            {
                m_Data = value;
            }
        }
        #endregion

    }
}