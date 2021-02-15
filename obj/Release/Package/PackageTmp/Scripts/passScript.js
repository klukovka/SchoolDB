function Show() {
    check = document.getElementById("show");
    pass = document.getElementById("pass")

    if (check.checked)
        pass.type = "text";
    else
        pass.type = "password";

}