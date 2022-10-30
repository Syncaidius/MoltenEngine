
class ObjectLoader extends BaseLoader {
    loadContent(elPage, dataNode, docPath) {
        this.buildMemberSection(elPage, dataNode, docPath, "Class");
        this.buildMemberSection(elPage, dataNode, docPath, "Struct");
        this.buildMemberSection(elPage, dataNode, docPath, "Field");
        this.buildMemberSection(elPage, dataNode, docPath, "Property");
        this.buildMemberSection(elPage, dataNode, docPath, "Method");
        this.buildMemberSection(elPage, dataNode, docPath, "Event");
    }
}