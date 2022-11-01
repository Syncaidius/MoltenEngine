
class BaseLoader {
    manager = null;

    constructor(docManager) {
        this.manager = docManager;
    }

    /* elPageName = The name of the element that will contain help page content.
     * title = The title of the loaded page
     * dataNode = The docData node that contains the information we need to display on the page.
     * docPath = The data path we used to reach the current dataNode. 
     */
    load(elPageName, title, dataNode, docPath) {
        let elPage = $(`#${elPageName}`);
        if (elPage == null) {
            console.error(`Page element '${elPageName}' not found`);
            return;
        }

        let pathParts = this.getPathParts(docPath);
        let lastPart = pathParts.length - 1;
        let pathHtml = "";
        let curPath = "";

        pathParts.forEach((part, index) => {
            if (index > 0) {
                pathHtml += ".";
                curPath += ".";
            }

            curPath += part;

            if (index < lastPart) {
                pathHtml += this.getDocTarget(curPath, "", part);
            } else {
                pathHtml += `<b>${title}</b>`;
            };
        });

        let iconHtml = this.manager.getIcon(dataNode);
        let summary = dataNode.Summary != null && dataNode.Summary.length > 0 ? dataNode.Summary : "No summary.";

        let inheritHtml = this.buildInheritChainHtml(dataNode);
        if (inheritHtml != null && inheritHtml.length > 0)
            inheritHtml = ` - Inherits: ${inheritHtml}`;

        elPage.html(`
            <div class="page-header">
                <div class="page-title">${iconHtml}<span id="page-title-span">${pathHtml}</span></div>
                <div class="page-info"><span class="page-inherit">${dataNode.DocType}${inheritHtml}</span></div>
            </div>
            <div id="${elPageName}-inner" class="scrollable sec-content"></div>
        `);

        this.manager.registerDocTargets(elPage);
        elPage = $(`#${elPageName}-inner`);
        elPage.append(`<div class="obj-summary"><p>${summary}</p>`);           

        this.loadContent(elPage, dataNode, docPath);
        this.manager.registerDocTargets(elPage);
    }

    loadContent(elPage, dataNode, docPath) { }

    getPathParts(docPath,) {
        return docPath.split(".");
    }

    getDocTarget(targetPath, memberName, title, extraClasses = null) {
        return `<a class="doc-target ${(extraClasses || "")}" data-target="${targetPath}" data-target-sec="${memberName}">${title}</a>`;
    }

    getDocInvalid(title) {
        return `<b class="doc-invalid" title="Invalid object name">${title}</b>`;
    }

    buildMemberSection(elPage, dataNode, docPath, docTypeFilter) {
        if (dataNode.Members == null || dataNode.Members.length == 0)
            return;

        let thisLoader = this;
        let title = this.manager.toPlural(docTypeFilter);
        let filtered = this.filterMembers(dataNode, docTypeFilter);
        if (filtered.length > 0) {
            let memberHtml = "";

            filtered.forEach((mName, index) => {
                let memberVariants = dataNode.Members[mName];
                for (let i = 0; i < memberVariants.length; i++) {
                    let member = memberVariants[i];
                    let targetPath = `${docPath}.${mName}`;
                    let summary = member.Summary != null ? member.Summary : "&nbsp;";
                    let icon = thisLoader.manager.getIcon(member);

                    let docTarget = this.getDocTarget(targetPath, mName, mName);
                    if (member.DocType == "Method" || member.DocType == "Constructor")
                        docTarget += this.buildParameterHtml(member);

                    memberHtml += `
                    <tr>
                        <td>${icon}</td>
                        <td><span>${docTarget}</span></td>
                        <td>${summary}</td>
                    </tr>
                `;
                }
            });


            elPage.append(`
                <div class="obj-section">
                    <h2>${title}</h2>
                    <table>
                        <thead>
                            <tr>
                            <tr>
                                <th width="20px">&nbsp</th>
                                <th width="30%">Name</th>
                                <th>Summary</th>
                            </tr>
                        </thead>
                        <tbody>${memberHtml}</tbody>
                    </table>
                </div>
                `);
        }
    }

    buildInheritChainHtml(dataNode) {
        if (dataNode.BaseTypeName == null)
            return "";

        let html = "";
        let basePath = dataNode.BaseTypeName;
        let baseNode = this.manager.getNode(basePath);
        if (baseNode == null)
            return;

        let first = true;
        while (baseNode != null) {
            if (first == false)
                html = ' \u25B6 ' + html;

            first = false;

            let title = this.manager.getPathTitle(basePath);
            html = this.getDocTarget(basePath, "", title) + html;

            // Get next base node
            basePath = baseNode.BaseTypeName;
            baseNode = this.manager.getNode(basePath);
        }

        if(first == true)
            html = this.getDocInvalid(title);

        return html;
    }

    buildParameterHtml(dataNode) {
        if (dataNode.Parameters == null || dataNode.Parameters.length == 0)
            return "()";

        let html = "";
        dataNode.Parameters.forEach((pNode, index) => {
            let pType = this.getPathParts(pNode.TypeName);
            let pTitle = pType[pType.length - 1];

            if (index > 0)
                html += ", ";
            html += this.getDocTarget(pNode.TypeName, pNode.Name, pTitle, "doc-parameter");
        });

        return `(${html})`;
    }

    filterMembers(dataNode, docTypeFilter) {
        let memberNames = Object.keys(dataNode.Members);
        let filtered = [];

        memberNames.forEach((mName, index) => {
            let variants = dataNode.Members[mName];
            if (variants.length == 0)
                return;

            if (variants[0].DocType == docTypeFilter)
                filtered.push(mName);

        });

        return filtered;
    }
}