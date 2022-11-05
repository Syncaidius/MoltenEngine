
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

        let inheritHtml = this.buildInheritChainHtml(dataNode);
        if (inheritHtml != null && inheritHtml.length > 0)
            inheritHtml = ` - Inherits: ${inheritHtml}`;

        let modifier = "";
        if (dataNode.IsStatic == true)
            modifier += "Static ";

        if (dataNode.IsVirtual == true)
            modifier += "Virtual ";

        if (dataNode.IsAbstract == true && dataNode.IsStatic != true)
            modifier += "Abstract ";

        elPage.html(`
            <div class="page-header">
                <div class="page-title">${iconHtml}<span id="page-title-span">${pathHtml}</span></div>
                <div class="page-info"><span class="page-inherit"><i class="keyword">${modifier}${dataNode.DocType}</i>${inheritHtml}</span></div>
            </div>
            <div id="${elPageName}-inner" class="scrollable sec-content"></div>
        `);

        this.manager.registerDocTargets(elPage);
        elPage = $(`#${elPageName}-inner`);
        this.loadIntro(elPage, dataNode, docPath);
        this.loadContent(elPage, dataNode, docPath);
        this.buildRemarkSection(elPage, dataNode);

        this.manager.registerDocTargets(elPage);
    }

    loadContent(elPage, dataNode, docPath) { }

    loadIntro(elPage, dataNode, docPath) {
        let summary = "";

        if (dataNode.Summary == null || dataNode.Summary.length == 0)
            summary = "No summary.";
        else
            summary = dataNode.Summary;

        elPage.append(`<div class="obj-summary"><p>${summary}</p>`);   
    }

    getPathParts(docPath) {
        return docPath.split(".");
    }

    getParentPath(docPath) {
        let parts = this.getPathParts(docPath);
        parts.pop();
        return parts.join('.');
    }

    /* targetPath = The target tree path
     * memberName = The member of the target path to navigate to. e.g. a method name.
     * title = The text displayed in place of the doc-target link.
     * extraClasses = Extra css classes to apply to the target html.
     * targetIndex = The variant/member index of given memberName. e.g. a method overload ID.
     */
    getDocTarget(targetPath, memberName, title, targetIndex = 0, extraClasses = null) {
        let targetID = "";
        if (targetIndex != null && targetIndex > 0)
            targetID = ` data-target-id="${targetIndex}"`;

        return `<a class="doc-target ${(extraClasses || "")}" data-target="${targetPath}"${targetID}>${title}</a>`;
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

                    let docTarget = this.getDocTarget(targetPath, mName, mName, i);
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
                                <th width="35%">Name</th>
                                <th>Summary</th>
                            </tr>
                        </thead>
                        <tbody>${memberHtml}</tbody>
                    </table>
                </div>
                `);
        }
    }

    buildRemarkSection(elPage, dataNode) {
        if (dataNode.Remark == null)
            return;

        elPage.append(`
            <div class="obj-section">
                <h2>Remarks</h2>
                <p>${dataNode.Remark}</p>
            </div>
            `);
    }

    getParameterKeyword(paramNode) {
        let keyword = "";

        if (paramNode.Keyword != null) {
            keyword = paramNode.Keyword.toLowerCase();
            keyword = `<b class="keyword">${keyword}</b> `;
        }

        return keyword;
    }

    buildInheritChainHtml(dataNode) {
        if (dataNode.BaseName == null)
            return "";

        let html = "";
        let basePath = dataNode.BaseName;
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
            basePath = baseNode.BaseName;
            baseNode = this.manager.getNode(basePath);
        }

        if(first == true)
            html = this.getDocInvalid(title);

        return html;
    }

    buildParameterHtml(dataNode) {
        if (dataNode.Params == null || dataNode.Params.length == 0)
            return "()";

        let html = "";
        dataNode.Params.forEach((pNode, index) => {
            let pType = this.getPathParts(pNode.TypeName);
            let pTitle = pType[pType.length - 1];
            let keyword = this.getParameterKeyword(pNode);

            if (index > 0)
                html += ", ";

            let docTarget = this.getDocTarget(pNode.TypeName, pNode.Name, pTitle, 0, "doc-parameter");
            html += `${keyword}${docTarget}`;
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

        filtered = filtered.sort(this.sortStrings);

        return filtered;
    }
}