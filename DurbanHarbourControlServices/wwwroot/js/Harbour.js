$(function () {
    var $rad = $('#rad'),
        $obj = $('.obj'),
        deg = 0;

    $obj.each(function () {
        var pos = $(this).data();

        // Read/set positions and store degree
        $(this).css({ left: pos.x, top: pos.y });

        setVesselAttr(this, pos.x, pos.y);
    });

    (function rotate() {
        $rad.css({ transform: 'rotate(' + deg + 'deg)' }); // Radar rotation
        var dot = $('[data-atDeg=' + deg + ']');
        var color = dot.css("background-color");
        dot
            .stop()
            .css({ background: '#fff' })
            .fadeTo(0, 1)
            .fadeTo(1700, 0.5, function () {
                dot.stop().css({ 'background-color': color });
            }); // Animate dot at deg

        deg = ++deg % 360;      // Increment and reset to 0 at 360
        setTimeout(rotate, 25); // LOOP
    })();
});

function setVesselAttr(vobj, x, y) {
    var rad = $('#rad').width() / 2,
        getAtan = Math.atan2(x - rad, y - rad),
        getDeg = (-getAtan / (Math.PI / 180) + 180) | 0;

    $(vobj).attr('data-atDeg', getDeg);
}