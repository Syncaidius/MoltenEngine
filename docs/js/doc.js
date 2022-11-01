

class DocManager {
    objTypes = ["Class", "Struct", "Enum", "Interface"];
    loaders = {};
    data = null;

    constructor(srcData) {
        this.data = srcData;
        this.loaders["Class"] = new ObjectLoader(this);
        this.loaders["Struct"] = this.loaders["Class"];
        this.loaders["Interface"] = this.loaders["Class"];
        this.loaders["Enum"] = this.loaders["Class"];
        this.loaders["Namespace"] = new NamespaceLoader(this);
        this.loaders["Method"] = new MethodLoader(this);
    }

    populateIndex() {
        let thisLoader = this;
        let keys = Object.keys(this.data.Members);
        let di = $("#doc-index");
        let treePath = [di];

        di.html("");
        keys = keys.sort(this.sortStrings);

        keys.forEach((key, index) => {
            let memList = thisLoader.data.Members[key];
            if (memList.length > 0)
                thisLoader.buildTreeNode(di, key, "", memList[0], treePath);
        });
    }

    buildEndNode(el, title, parentPath, dataNode) {
        let thisLoader = this;
        let curPath = `${parentPath}${(parentPath.length > 0 && title ? "." : "")}${title}`;
        let idName = this.toIDName(curPath);
        let targetName = this.toIDName(title);

        let iconHtml = this.getIcon(dataNode);
        el.append(` <div id="t-${idName}" class="doc-target" data-target="${parentPath}" data-target-sec="${targetName}">
                    ${iconHtml}
                    <a>${title}</a>
                </div>`);

        let target = $(`#t-${idName}`);
        target.on("click", function (e) {
            let nodePath = target.data("target");
            let node = thisLoader.getNode(nodePath);
            let loader = thisLoader.loaders[node.DocType];
            if (loader == null)
                return;

            let pTitle = thisLoader.getPathTitle(nodePath);
            loader.load("main-page", pTitle, node, nodePath);
        });
    }

    buildTreeNode(el, title, parentPath, dataNode, treePath, empty = false) {
        let curPath = `${parentPath}${(parentPath.length > 0 && title ? "." : "")}${title}`;
        if (dataNode.DocType == "Namespace")
            title = curPath;

        let idName = this.toIDName(curPath);
        let dataTarget = empty == true ? "" : `data-target="${curPath}"`;
        el.append(`<div id="i-${idName}" class="sec-namespace${(treePath.length > 1 ? "-noleft" : "")}">
                    <span class="namespace-toggle\" ${dataTarget}>${title}</span><br/>
                    <div id="in-${idName}" class="sec-namespace-inner"></div>
                </div>`);

        let elInner = $(`#in-${idName}`);

        if (!dataNode.Members || empty == true)
            return elInner;

        let thisLoader = this;
        let keys = Object.keys(dataNode.Members);
        let nextTreePath = [...treePath, elInner];

        keys = keys.sort(this.sortStrings);
        keys.forEach((mName, index) => {
            let memberArray = dataNode.Members[mName];
            if (memberArray.length == 0)
                return;

            // We're building an index tree, so we only need to know about the first entry of each member, to avoid duplicate index listings.
            let memberNode = memberArray[0];
            let memType = memberNode.DocType;

            switch (memType) {
                case "Namespace":
                    let resetTreePath = [nextTreePath[0]]; // Go back to root of path
                    thisLoader.buildTreeNode(nextTreePath[0], mName, curPath, memberNode, resetTreePath);
                    break;

                case "Struct":
                case "Interface":
                case "Class":
                case "Enum":
                    let categoryName = thisLoader.toPlural(memType);
                    thisLoader.buildCategorizedNode(elInner, categoryName, mName, curPath, memberNode, nextTreePath);
                    break;

                case "Field":
                case "Property":
                case "Event":
                case "Constructor":
                case "Method":
                    let endName = thisLoader.toPlural(memType);
                    thisLoader.buildCategorizedNode(elInner, endName, mName, curPath, memberNode, nextTreePath, true);
                    break;
            }
        });

        return elInner;
    }

    buildCategorizedNode(elParent, category, title, parentPath, dataNode, treePath, isEnd = false) {
        elParent.categories = elParent.categories || {};

        if (elParent.categories[category] == null)
            elParent.categories[category] = this.buildTreeNode(elParent, category, parentPath, dataNode, treePath, true);

        let elInner = elParent.categories[category];
        let nextTreePath = [...treePath, elInner];
        let curPath = `${parentPath}${(parentPath.length > 0 && category ? "." : "")}${category}`;

        if (isEnd == true)
            this.buildEndNode(elInner, title, parentPath, dataNode, nextTreePath);
        else
            this.buildTreeNode(elInner, title, parentPath, dataNode, nextTreePath);
    }

    toPlural(word) {
        if (word.length == 0)
            return word;

        let last = word[word.length - 1].toLowerCase();
        switch (last) {
            case 'y':
                return word.substring(0, word.length - 1) + "ies";

            case 'h':
            case 's':
            case 'x':
            case 'z':
                return word.substring(0, word.length) + "es";
        }

        return word + "s";
    }

    toIDName(str) {
        return str.replace(/(\.|>|<|,| )/g, '-');
    }

    toHtml(str) {
        return str.replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }

    getIcon(docNode) {
        return `<img class="doc-icon" src="docs/img/${docNode.DocType.toLowerCase()}.png" title="${docNode.DocType}"
/>`;
    }

    sortStrings(a, b) {
        if (a > b)
            return 1;

        if (a < b)
            return -1;

        return 0;
    }

    getNode(nodePath) {
        if (nodePath == null || nodePath.length == 0)
            return null;

        let parts = nodePath.split(".");

        let node = docData;
        parts.forEach((p, index) => {
            let next = node.Members[p];

            if (next != null && next.length > 0)
                node = next[0];
        });

        return node;
    }

    getPathTitle(nodePath) {
        let parts = nodePath.split(".");
        return parts.length > 0 ? parts[parts.length - 1] : "[No Title]";
    }

    loadPage(target) {
        let nodePath = target.data("target");
        if (nodePath == null)
            return;

        let node = this.getNode(nodePath);
        let loader = this.loaders[node.DocType];

        if (loader == null) {
            console.log(`No loader for path "${nodePath}"`);
            return;
        }

        let pTitle = this.getPathTitle(nodePath);
        loader.load("main-page", pTitle, node, nodePath);
    }

    registerDocTargets(parent) {
        let thisLoader = this;

        parent.find(".doc-target").on("click", function (e) {
            let target = $(e.target);
            thisLoader.loadPage(target);
        });
    }
}

$(document).ready(function () {
    // Set page title
    $('#doc-title').html(docData.Name);
    $('#doc-intro').html(docData.Intro);

    let manager = new DocManager(docData);

    manager.populateIndex();

    let toggler = document.getElementsByClassName("namespace-toggle");
    let i;

    for (i = 0; i < toggler.length; i++) {
        toggler[i].addEventListener("click", function () {
            {
                this.parentElement.querySelector(".sec-namespace-inner").classList.toggle("sec-active");
                this.classList.toggle("namespace-toggle-down");

                let target = $(this);
                manager.loadPage(target);
            }
        });
    }
})
