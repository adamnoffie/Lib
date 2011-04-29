<%@ Page Language="C#" MasterPageFile="~/Admin/AdminMaster.master" AutoEventWireup="true" 
    CodeFile="PhotosAdd.aspx.cs" Inherits="Admin_PhotosAdd" Title="Add Photos" %>

<asp:Content ID="head" ContentPlaceHolderID="cpH" Runat="Server">
    <link href="uploadify/uploadify.css" rel="stylesheet" type="text/css" />
    <script src="uploadify/jquery.uploadify.v2.1.0.min.js" type="text/javascript"></script>
    <script src="uploadify/swfobject.js" type="text/javascript"></script>
    
    <style type="text/css">
        #upControls > span, #upControls > object { float: left }
        #upControls input { height: 30px }
        #upControls span { line-height: 30px }
        #upImgQueue { clear: both; padding-top: 2px }
        
        .photo-add { width: 200px; padding: 6px 6px 20px; float: left; margin-right: 10px }
        .photo-add-imgwrap { width: 200px; height: 160px; text-align: center; position: relative }        
        .photo-add-imgwrap > a { position: absolute; top: 4px; right: 4px;  border: 1px solid gray;
            padding: 4px 4px 4px 18px; background-color: white !important; display: none }
        .photo-add-imgwrap:hover > a { display: block }
        .photo-add-imgwrap > a { -moz-border-radius: 4px; -webkit-border-radius: 4px; }
        .photo-add img { max-width: 100%; max-height: 150px; }
        .photo-add textarea, .photo-add input { width: 186px !important }
        .photo-add textarea { height: 84px }
    </style>
</asp:Content>

<asp:Content ID="body" ContentPlaceHolderID="cpM" Runat="Server">
<div class="content-box">
<div class="content-box-content">
<asp:MultiView ID="mview" runat="server" ActiveViewIndex="0">

<%-------------- upload 1 or more photos -------------------------%>
<asp:View ID="vwUpload" runat="server" >
    
    <h3>Step 1: Select Images to Upload</h3>
    <h3 class="withFiles" style="display: none">Step 2: Click <em>Upload Images</em></h3>

    <div id="upControls">
    <input id="upImg" name="upImg" type="file" />    
        <span class="withFiles" style="float: left; padding-left: 6px; display: none">
            <input id="upload" name="upload" type="button" value="Upload Images" class="button"
                onclick="$('#upImg').uploadifyUpload();" />
            <a id="clear" class="iconify del"
                href="javascript:$('#upImg').uploadifyClearQueue(); showWithFiles(false);">
                Clear Queue
            </a>
        </span>
    </div>
    <div id="upImgQueue"></div>
    
    
    <script type="text/javascript">
        $(function() {
            $('#upImg').uploadify({
                'uploader': 'uploadify/uploadify.swf',
                'script': 'UploadifyImages.ashx',
                'multi': 'true',
                'sizeLimit': '10485760', // 10MB
                'fileDesc': 'Select image file(s) to upload',
                'fileExt': '*.gif;*.jpg;*.jpeg;*.png;*.bmp',
                'queueID': 'upImgQueue',
                'buttonText': 'SELECT IMAGES',
                'cancelImg': 'simpla/images/icons/cross.png',

                'onError': function(event, queueID, fileObj, errorObj) {
                    alert('type: ' + errorObj.type + '\r\n info: ' + errorObj.info);
                },
                'onSelectOnce': function(event, data) {
                    showWithFiles(data.fileCount > 0);
                },
                'onCancel': function(event, queueID, fileObj, data) {
                    showWithFiles(data.fileCount > 0);
                },
                'onAllComplete': function(event, data) {
                    if (data.errors == 0) window.location = '<%= Request.RawUrl %>';
                }
            });
        });                

        function showWithFiles(show) {
            if (show) $('.withFiles').show();
            else $('.withFiles').hide();
        }
    </script>
</asp:View>

<%--------- view photos that were uploaded, give them names/description, submit to album -----%>
<asp:View ID="vwAdd" runat="server" >
<h3>Step 3: Select an Album to add to, or Create one</h3>
<p>
    <asp:RadioButton ID="radAlbumNew" runat="server" Checked="true" GroupName="album" />
    Create New: 
    <asp:TextBox ID="txtAlbumName" runat="server" CssClass="text-input" />
    
    <span id="spanAlbumSelect" runat="server" visible="false" >
        <asp:RadioButton ID="radAlbumSelect" runat="server" GroupName="album" />
        Add to Album: 
        <asp:DropDownList ID="ddAlbums" runat="server" />
        
        <%--- put this here because it is not necessary if there are no existing albums ---%>
        <script type="text/javascript">
            $(function() {
                $('input[type=radio][name$=album]').click(radAlbum_Click);
                radAlbum_Click();
            });
            function radAlbum_Click() {
                txtAlbumName = $('#<%= txtAlbumName.ClientID %>');
                if ($('input[type=radio][name$=album]:checked').val() == 'radAlbumNew') 
                    txtAlbumName.removeAttr('disabled');
                else
                    txtAlbumName.attr('disabled', 'disabled');
            }
        </script>
    </span>
</p>

<h3>Step 4: Enter Title/Descriptions</h3>
<p>Note: Hover over an image and select "Cancel" to remove the image from this import.</p>

<asp:Repeater ID="rptFiles" runat="server" OnItemDataBound="rptFiles_ItemDataBound">
<ItemTemplate>
<div class="photo-add">
    <div class="photo-add-imgwrap">
        <a id="lnkDel" runat="server" class="iconify del">cancel</a>
        <asp:Image ID="img" runat="server" />
    </div>
    Title: <br />
    <asp:TextBox ID="txtName" runat="server" CssClass="text-input" />
    Description: <br />
    <asp:TextBox ID="txtDesc" runat="server" TextMode="MultiLine" />
    
    <asp:HiddenField ID="hdnName" runat="server" />
    <asp:HiddenField ID="hdnDel" runat="server" />
</div>
</ItemTemplate>
</asp:Repeater>
<div class="clear"></div>
<asp:Button ID="btnSubmit" runat="server" Text="Save Photos" CssClass="button bold" OnClick="btnSubmit_Click" />
<asp:Button ID="btnCancel" runat="server" Text="Cancel Upload" CssClass="button" OnClick="btnCancel_Click" />



<script type="text/javascript">
function del(hdnID) {
    // mark photo as deleted, and fade it out
    $('#' + hdnID).val('true').parent().fadeOut('fast');
}
$(function() {
    // if page is reloaded, hide all of the ones marked for removal in browsers that persist forms
    $('.photo-add > input[id$=hdnDel][value=true]').parent().hide();
});
</script>

</asp:View>
</asp:MultiView>
</div>
</div>
</asp:Content>

