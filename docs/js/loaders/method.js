
class MethodLoader extends BaseLoader {
    loadContent(elPage, dataNode, docPath) {

        let paramHtml = "";

        if (dataNode.Parameters == null || dataNode.Parameters.length == 0)
            return;

        dataNode.Parameters.forEach((pNode, index) => {
            let typeName = this.manager.getNode(pNode.TypeName);
            let typeTitle = this.manager.getPathTitle(pNode.TypeName);
            let typeTarget = "";

            if (typeName != null)
                typeTarget = this.getDocTarget(pNode.TypeName, "", typeTitle);
            else
                typeTarget = this.getDocInvalid(pNode.TypeName);

            let summary = pNode.Summary != null ? pNode.Summary : "&nbsp";

            paramHtml += `
            <tr>
                <td></td>
                <td>${pNode.Name}</td>
                <td>${typeTarget}</td>
                <td>${summary}</td>
            </tr>`;
        });

        elPage.append(`
            <div class="obj-section">
                <h2>Parameters</h2>
                <table>
                    <thead>
                        <tr>
                        <tr>
                            <th width="20px">&nbsp</th>
                            <th width="30%">Name</th>
                            <th width="20%">Type</th>
                            <th>Summary</th>
                        </tr>
                    </thead>
                    <tbody>${paramHtml}</tbody>
                </table>
            </div>
        `);
    }
}