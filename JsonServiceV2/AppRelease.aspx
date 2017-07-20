<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AppRelease.aspx.cs" Inherits="JsonService.AppRelease" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <a>请输入版本号:</a><asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
    <a>(如3.4)</a><br />
        <br />
        <a>请选择APK文件:</a><asp:FileUpload ID="FileUpload1" runat="server" />
        <br />
        <br />
        &nbsp;&nbsp;&nbsp;
        <asp:Button ID="Button1" runat="server" Text="上传" onclick="Button1_Click" />
    
    </div>
    </form>
</body>
</html>
