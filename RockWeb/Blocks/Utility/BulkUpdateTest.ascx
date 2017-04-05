<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkUpdateTest.ascx.cs" Inherits="RockWeb.Blocks.Utility.BulkUpdateTest" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Bulk Update Cleanup</h1>
            </div>
            
            <div class="panel-body">
                <asp:LinkButton ID="btnCleanup" runat="server" CssClass="btn btn-default" Text="Cleanup" OnClick="btnCleanup_Click" />
            </div>

            <Rock:NotificationBox ID="nbResults" runat="server" NotificationBoxType="Success" Dismissable="true" />
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>