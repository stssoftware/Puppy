﻿![Logo](favicon.ico)
# Puppy.DataTable Document
> Project Created by [**Top Nguyen**](http://topnguyen.net)

- This is a flork and continue develop of project [mvc.jquery.datatables]("https://github.com/mcintyre321/mvc.jquery.datatables")
- Improve things:
    + Coding smell
    + Coding Convention
    + Improve query performance
    + Support for real cases

- Example: http://aspdatatables.azurewebsites.net/

### Changing case sensitivity

```csharp
@using QueryInterceptor

...

public DataTablesResult<TDataTableRow> GetDataTableData(DataTablesParam dataTableParam)
{
    var originalQueryable = ... some code to get your IQueryable<TDataTableRow> ...;
    var caseInsenstitiveQueryable = originalQueryable.InterceptWith(new SetComparerExpressionVisitor(StringComparison.CurrentCultureIgnoreCase));

    return DataTablesResult.Create(caseInsenstitiveQueryable, dataTableParam);
}
```

### Customising column rendering
```csharp
public class Message
{
    public DateTime CreatedDate{get;set;}
    public User User {get;set;} 
    public ICollection<Recipients> Recipients {get; set;}
    public string Text {get;set;}
}

public class MessageViewModel
{
    public DateTime CreatedDate { get; set; }
    public string User { get; set; } 
     // Dont want this as a column, we are keeping it here so we can use it in the transform 
    [DataTablesExclude]
    public User UserEntity {get;set;} 
    public string Text {get;set;}
}

[HttpPost]
ActionResult GetMessagesDataRows(DataTablesParam param)
{
    IQueryable<Message> messages = db.Query<Message>();
    IQueryable<MessageViewModel> messageViewModels = messages.Select(m => new MessageViewModel {
        CreatedDate = m.CreatedDate,
        User = m.User.Name,
        Text = m.Text
    });

    return DataTablesResult.Create(messages, param, x => new  {
        CreatedDate = x.CreatedDate.ToFriendlyTimeString(), 
        User = string.Format("<a href='/users/{0}'>{1}</a>", r.UserEnt.Id, r.UserEnt.Name)
    });
});
```

### Specifying Initial Search Values
```csharp
     vm
        .FilterOn("Position", new { sSelector = "#custom-filter-placeholder-position" }, new { sSearch = "Tester" }).Select("Engineer", "Tester", "Manager")

        .FilterOn("Id", null, new { sSearch = "2~4", bEscapeRegex = false }).NumberRange()

        .FilterOn("IsAdmin", null, new { sSearch = "False" }).Select("True","False")

        .FilterOn("Salary", new { sSelector = "#custom-filter-placeholder-salary" }, new { sSearch = "1000~100000" }).NumberRange();

    vm.StateSave = false;
```

# Main.cshtml
```html
@using Monkey.Controllers.Api
@using Puppy.DataTable
@using Puppy.DataTable.Helpers
@using Constants = Monkey.Constants
@{
    ViewData[Constants.ViewDataKey.Title] = "Portal";
}

<div class="page-header">
    <h1 class="page-title">DataTables</h1>
</div>
<div class="page-content">
    <div class="panel">
        <div class="panel-body">

            @{
                var model = Html.DataTableModel(Guid.NewGuid().ToString("N"), (TestApiController controller) => controller.GetFacetedUsers(null));
                model.IsStateSave = false;
                model.IsUseTableTools = true;
                model.IsUseColumnFilterPlugin = true;
                model.IsDevelopMode = true;
                model.TableClass = "table table-hover dataTable table-striped w-full";
            }

            @await Html.PartialAsync("/Areas/Portal/Views/Shared/_DataTable.cshtml", model).ConfigureAwait(true)
        </div>
    </div>
</div>
```


# _DataTable.cshtml
```html
@using Newtonsoft.Json.Linq
@using Puppy.DataTable.Models
@model DataTableConfigModel

<table id="@Model.Id" class="@(Model.TableClass ?? string.Empty)">
    <thead>
        @if (Model.IsUseColumnFilter)
        {
            <tr>
                @foreach (var column in Model.Columns)
                {
                    <th>@column.DisplayName</th>
                }
            </tr>
        }
        @if (!Model.IsHideHeader)
        {
            <tr>
                @foreach (var column in Model.Columns)
                {
                    <th class="@column.CssClassHeader">@column.DisplayName</th>
                }
            </tr>
        }
    </thead>
    <tbody>
        <tr>
            <td colspan="@Model.Columns.Count()" class="dataTables_empty">
                Loading...
            </td>
        </tr>
    </tbody>
</table>

<script type="text/javascript">
    (function initDataTable() {
        if (!window.jQuery || !$.fn.DataTable) {
            setTimeout(initDataTable, 100);
            return;
        }
        var $table = $('#@Model.Id');
        @{
            var options = new JObject
            {
                ["aaSorting"] = new JRaw(Model.ColumnSortingString),
                ["bProcessing"] = true,
                ["stateSave"] = Model.IsStateSave,
                ["stateDuration"] = -1,
                ["bServerSide"] = true,
                ["bFilter"] = Model.IsShowGlobalSearchInput,
                ["sDom"] = Model.Dom,
                ["responsive"] = Model.IsResponsive,
                ["language"] = new JObject
                {
                    ["search"] = "_INPUT_",
                    ["lengthMenu"] = "_MENU_",
                    ["sSearchPlaceholder"] = "Search...",
                },
                ["bAutoWidth"] = Model.IsAutoWidthColumn,
                ["sAjaxSource"] = Model.AjaxUrl,
                ["aoColumnDefs"] = new JRaw(Model.ColumnDefsString),
                ["aoSearchCols"] = Model.SearchCols,
                // Size List
                ["aLengthMenu"] = Model.LengthMenu != null ? new JRaw(Model.LengthMenu) : new JRaw(string.Empty)
            };

            // Default Size
            if (Model.PageSize.HasValue)
            {
                options["iDisplayLength"] = Model.PageSize;
            }

            // Language Code
            if (!string.IsNullOrWhiteSpace(Model.LanguageCode))
            {
                options["oLanguage"] = new JRaw(Model.LanguageCode);
            }

            // Draw Call back function
            if (!string.IsNullOrWhiteSpace(Model.DrawCallback))
            {
                options["fnDrawCallback"] = new JRaw(Model.DrawCallback);
            }

            // Server Request
            options["fnServerData"] = new JRaw(
                "function(sSource, aoData, fnCallback) { " +
                "    var ajaxOptions = { 'dataType': 'json', 'type': 'POST', 'url': sSource, 'data': aoData, 'success': fnCallback }; " +
                (Model.AjaxErrorHandler == null ? "" : ("ajaxOptions['error'] = " + Model.AjaxErrorHandler) + "; ") +
                "    $.ajax(ajaxOptions);" +
                "}");

            // Tools
            if (Model.IsUseTableTools)
            {
                options["oTableTools"] = new JRaw("{ 'sSwfPath': '" + Url.AbsoluteContent("~/portal/vendor/datatables-tabletools/swf/copy_csv_xls_pdf.swf") + "' }");

                string tools = Model.IsEnableColVis ? "{extend: 'colvis', text: 'Columns'}," : string.Empty;
                tools += "'copy', 'excel', 'csv', 'pdf', 'print'";
                options["buttons"] = new JRaw($"[{tools}]");
            }

            // Additional Option
            if (Model.AdditionalOptions.Any())
            {
                foreach (var jsOption in Model.AdditionalOptions)
                {
                    options[jsOption.Key] = new JRaw(jsOption.Value);
                }
            }
        }

        var dataTableOptions = @Html.Raw(options.ToString(Formatting.Indented));

        @if (Model.IsDevelopMode)
        {
            @Html.Raw("console.log(dataTableOptions);")
        }

        var $dataTable = $table.dataTable(dataTableOptions);

        // Col filters
        @if (Model.IsUseColumnFilter)
        {
            @Html.Raw("$dataTable.columnFilter(" + Model.ColumnFilterVm + ");")
        }

        // Global Variable
        @if (!string.IsNullOrWhiteSpace(Model.GlobalJsVariableName))
        {
            @Html.Raw("window['" + Model.GlobalJsVariableName + "'] = $dataTable;")
        }
    })();
</script>
```