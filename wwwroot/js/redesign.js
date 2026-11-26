(function () {
    'use strict';

    // ---------- 1. Шапка зменшується, коли сторінку прокрутили ----------
    var header = document.getElementById('siteHeader');
    var ticking = false;

    function updateHeader() {
        var y = window.pageYOffset || document.documentElement.scrollTop || 0;
        // два різні пороги (60 і 20), щоб шапка не мигала біля межі
        if (!header.classList.contains('is-scrolled') && y > 60) {
            header.classList.add('is-scrolled');
        } else if (header.classList.contains('is-scrolled') && y < 20) {
            header.classList.remove('is-scrolled');
        }
        ticking = false;
    }

    if (header) {
        window.addEventListener('scroll', function () {
            if (!ticking) {
                window.requestAnimationFrame(updateHeader);
                ticking = true;
            }
        }, { passive: true });
        updateHeader();

        // Підсвічуємо поточний пункт меню
        var path = location.pathname.replace(/\/+$/, '').toLowerCase() || '/';
        if (path === '/home') path = '/';
        header.querySelectorAll('.site-main .nav-link').forEach(function (a) {
            if (a.hash) return; // пункти-якорі (#specialties) не підсвічуємо
            var p = a.pathname.replace(/\/+$/, '').toLowerCase() || '/';
            if (p === '/home' || p === '/home/index') p = '/';
            if (p === path) a.classList.add('active');
        });
    }

    // ---------- 2. Кнопка «Нагору» ----------
    var toTop = document.getElementById('toTop');
    if (toTop) {
        toTop.addEventListener('click', function (e) {
            e.preventDefault();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    // ---------- 3. Поява елементів при прокручуванні ----------
    function animateCount(el) {
        var target = parseInt(el.getAttribute('data-count'), 10);
        if (isNaN(target)) return;
        var duration = 1100;
        var start = null;
        function step(ts) {
            if (start === null) start = ts;
            var p = Math.min((ts - start) / duration, 1);
            var eased = 1 - Math.pow(1 - p, 3);
            el.textContent = Math.round(target * eased);
            if (p < 1) window.requestAnimationFrame(step);
        }
        window.requestAnimationFrame(step);
    }

    function reveal(el) {
        el.classList.add('is-visible');
        el.querySelectorAll('[data-count]').forEach(animateCount);
        if (el.hasAttribute('data-count')) animateCount(el);
    }

    var items = document.querySelectorAll('.reveal');
    if (!('IntersectionObserver' in window)) {
        items.forEach(reveal);
        return;
    }
    var io = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                reveal(entry.target);
                io.unobserve(entry.target);
            }
        });
    }, { threshold: 0.15, rootMargin: '0px 0px -40px 0px' });
    items.forEach(function (el) { io.observe(el); });
})();
