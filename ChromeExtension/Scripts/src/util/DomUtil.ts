class DomUtil {
    
    static getCommonAncestor(a: Element, b: Element) {
        var parentsa = $(a).parents().toArray();
        var parentsb = $(b).parents().toArray();

        parentsa.unshift(a);
        parentsb.unshift(b);

        var found = null;

        $.each(parentsa, function () {
            var thisa = this;

            $.each(parentsb, function () {
                if (thisa == this) {
                    found = this;
                    return false;
                }
            });

            if (found) return false;
        });

        return found;
    }

}