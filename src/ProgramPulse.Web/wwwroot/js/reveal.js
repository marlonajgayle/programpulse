// Reveal-on-scroll for marketing and auth sections (DESIGN_SYSTEM.md §3).
//
// Elements marked .rise start translated down and transparent; they animate to rest
// once 12% of them is in view, and are unobserved immediately after so they never
// re-animate on scroll-back.
//
// Blazor WebAssembly renders and re-renders into #app long after DOMContentLoaded, so
// a one-shot querySelectorAll would miss almost everything. A document-level
// MutationObserver picks up .rise nodes as Blazor commits them — that keeps this file
// free of any JS interop and means no component has to call into it.
(function () {
    'use strict';

    // Reduced motion: show everything at rest and never observe anything.
    // The matching CSS rule already forces the final state; this just avoids the work.
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        return;
    }

    var STAGGER_MS = 50;
    var MAX_STAGGER_STEPS = 6; // beyond ~300ms a stagger reads as lag, not rhythm

    var io = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (!entry.isIntersecting) return;

            var el = entry.target;

            // Stagger siblings so a row of cards arrives in sequence rather than as a slab.
            var index = 0;
            if (el.parentElement) {
                var siblings = el.parentElement.querySelectorAll(':scope > .rise');
                index = Math.min(
                    Array.prototype.indexOf.call(siblings, el),
                    MAX_STAGGER_STEPS
                );
            }
            el.style.transitionDelay = (index * STAGGER_MS) + 'ms';
            el.classList.add('is-in');

            io.unobserve(el);
        });
    }, { threshold: 0.12 });

    function observe(root) {
        if (root.nodeType !== 1) return;
        if (root.classList.contains('rise') && !root.classList.contains('is-in')) {
            io.observe(root);
        }
        root.querySelectorAll('.rise:not(.is-in)').forEach(function (el) {
            io.observe(el);
        });
    }

    observe(document.body);

    new MutationObserver(function (mutations) {
        mutations.forEach(function (m) {
            m.addedNodes.forEach(observe);
        });
    }).observe(document.body, { childList: true, subtree: true });
})();
