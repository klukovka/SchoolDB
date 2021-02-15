window.onload = $(function () {
    $('form#tree legend').on('click', function () {
        if ($(this).text() == "+") { $(this).text("-"); }
        else { $(this).text("+"); }
        //console.log(this);
        //console.log("click");
        var currthread = $(this).parent("fieldset");
        $(currthread).children("fieldset").toggleClass("closed");
    });

    $("input:checkbox").on('change', function () {
        var currcheck = $(this).closest("fieldset").has("fieldset");

        //отмечаем или снимаем отметку с детей      
        if ($(this).prop("checked") == true) {
            //console.log("true");
            $(currcheck).find("input:checkbox").prop({ "checked": true, "indeterminate": false }); //почему-то надо снимать "indeterminate"
        } else {
            //console.log("false");
            $(currcheck).find("input:checkbox").prop({ "checked": false, "indeterminate": false }); //почему-то надо снимать "indeterminate"

        }

        //все ПРЯМЫЕ предки чекбокса становятся indeterminate
        var currdeterm = $(this).closest("fieldset");
        $(currdeterm).parentsUntil($("form#tree"), "fieldset").each(function () {
            //console.log(this);
            $(this).find("input:checkbox:first").prop({ "indeterminate": true, "checked": false });
        });

        //теперь проверим всех родителей-fieldset'ов этого чекбокса до самого верха. Каждого родителя надо проверить на чекнутость детей

        $(this).parentsUntil($("form#tree"), "fieldset").has("fieldset").each(function () {

            var chkbx = $(this).find("input:checkbox").not(":first"); //выбираем всех чекбоксов-детей-внуков и т.д.
            var chkd = $(this).find("input:checkbox:checked"); //выбираем чекнутые чекбоксы среди детей

            //console.log("всего: " + chkbx.length + "," + "чекнутых: " + chkd.length);
            if (chkd.length == chkbx.length) { //если кол-во чекнутых равно общему кол-ву чекбоксов, то чекаем и этот чекбокс
                $(this).find("input:checkbox:first").prop({ "indeterminate": false, "checked": true });

            }


        });

    });

});