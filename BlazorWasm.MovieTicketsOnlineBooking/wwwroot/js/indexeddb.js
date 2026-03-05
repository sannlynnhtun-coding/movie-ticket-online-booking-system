// Cinematix IndexedDB module
// Manages booking drafts + purchased ticket persistence in the browser

const DB_NAME = 'CinematixDb';
const DB_VERSION = 2;
const STORE_TICKETS = 'tickets';
const STORE_BOOKING_DRAFTS = 'bookingDrafts';
const STORE_VOUCHER_HEADS = 'voucherHeads';

let _db = null;

function openDb() {
    return new Promise((resolve, reject) => {
        if (_db) {
            resolve(_db);
            return;
        }

        const req = indexedDB.open(DB_NAME, DB_VERSION);
        req.onupgradeneeded = (e) => {
            const db = e.target.result;

            if (!db.objectStoreNames.contains(STORE_TICKETS)) {
                const store = db.createObjectStore(STORE_TICKETS, { keyPath: 'bookingVoucherDetailId' });
                store.createIndex('bookingDate', 'bookingDate', { unique: false });
            }

            if (!db.objectStoreNames.contains(STORE_BOOKING_DRAFTS)) {
                const store = db.createObjectStore(STORE_BOOKING_DRAFTS, { keyPath: 'bookingId' });
                store.createIndex('scopeKey', 'scopeKey', { unique: false });
                store.createIndex('scopeKeySeatId', ['scopeKey', 'seatId'], { unique: true });
            }

            if (!db.objectStoreNames.contains(STORE_VOUCHER_HEADS)) {
                const store = db.createObjectStore(STORE_VOUCHER_HEADS, { keyPath: 'bookingVoucherHeadId' });
                store.createIndex('bookingDate', 'bookingDate', { unique: false });
            }
        };

        req.onsuccess = (e) => {
            _db = e.target.result;
            resolve(_db);
        };
        req.onerror = (e) => reject(e.target.error);
    });
}

window.cinematix = {
    initDb: async function () {
        await openDb();
        return true;
    },

    // Tickets (voucher details)
    saveTickets: async function (tickets) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_TICKETS, 'readwrite');
            const store = tx.objectStore(STORE_TICKETS);
            let count = 0;
            for (const ticket of tickets) {
                store.put(ticket);
                count++;
            }
            tx.oncomplete = () => resolve(count);
            tx.onerror = (e) => reject(e.target.error);
        });
    },

    getAllTickets: async function () {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_TICKETS, 'readonly');
            const store = tx.objectStore(STORE_TICKETS);
            const req = store.getAll();
            req.onsuccess = () => resolve(req.result);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    clearTickets: async function () {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_TICKETS, 'readwrite');
            const store = tx.objectStore(STORE_TICKETS);
            const req = store.clear();
            req.onsuccess = () => resolve(true);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    deleteTicket: async function (id) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_TICKETS, 'readwrite');
            const store = tx.objectStore(STORE_TICKETS);
            const req = store.delete(id);
            req.onsuccess = () => resolve(true);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    saveVoucherDetail: async function (detail) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_TICKETS, 'readwrite');
            const store = tx.objectStore(STORE_TICKETS);
            const req = store.put(detail);
            req.onsuccess = () => resolve(true);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    getVoucherDetails: async function () {
        return window.cinematix.getAllTickets();
    },

    // Voucher heads
    saveVoucherHead: async function (head) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_VOUCHER_HEADS, 'readwrite');
            const store = tx.objectStore(STORE_VOUCHER_HEADS);
            const req = store.put(head);
            req.onsuccess = () => resolve(true);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    getVoucherHeads: async function () {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_VOUCHER_HEADS, 'readonly');
            const store = tx.objectStore(STORE_VOUCHER_HEADS);
            const req = store.getAll();
            req.onsuccess = () => resolve(req.result);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    // Booking drafts
    saveBookingDraft: async function (draft) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_BOOKING_DRAFTS, 'readwrite');
            const store = tx.objectStore(STORE_BOOKING_DRAFTS);
            const req = store.put(draft);
            req.onsuccess = () => resolve(true);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    getBookingDraftsByScope: async function (scopeKey) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_BOOKING_DRAFTS, 'readonly');
            const store = tx.objectStore(STORE_BOOKING_DRAFTS);
            const idx = store.index('scopeKey');
            const req = idx.getAll(scopeKey);
            req.onsuccess = () => resolve(req.result);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    deleteBookingDraftBySeat: async function (scopeKey, seatId) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_BOOKING_DRAFTS, 'readwrite');
            const store = tx.objectStore(STORE_BOOKING_DRAFTS);
            const idx = store.index('scopeKeySeatId');
            const req = idx.getKey([scopeKey, seatId]);
            req.onsuccess = () => {
                if (req.result === undefined) {
                    resolve(true);
                    return;
                }
                const deleteReq = store.delete(req.result);
                deleteReq.onsuccess = () => resolve(true);
                deleteReq.onerror = (e) => reject(e.target.error);
            };
            req.onerror = (e) => reject(e.target.error);
        });
    },

    clearBookingDraftsByScope: async function (scopeKey) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_BOOKING_DRAFTS, 'readwrite');
            const store = tx.objectStore(STORE_BOOKING_DRAFTS);
            const idx = store.index('scopeKey');
            const req = idx.openCursor(scopeKey);
            req.onsuccess = (e) => {
                const cursor = e.target.result;
                if (cursor) {
                    cursor.delete();
                    cursor.continue();
                } else {
                    resolve(true);
                }
            };
            req.onerror = (e) => reject(e.target.error);
        });
    },

    clearAllBookingDrafts: async function () {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const tx = db.transaction(STORE_BOOKING_DRAFTS, 'readwrite');
            const store = tx.objectStore(STORE_BOOKING_DRAFTS);
            const req = store.clear();
            req.onsuccess = () => resolve(true);
            req.onerror = (e) => reject(e.target.error);
        });
    },

    // Theme helpers
    getTheme: function () {
        return localStorage.getItem('cx-theme') || 'dark';
    },
    setTheme: function (theme) {
        localStorage.setItem('cx-theme', theme);
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        return theme;
    },
    toggleTheme: function () {
        const current = window.cinematix.getTheme();
        return window.cinematix.setTheme(current === 'dark' ? 'light' : 'dark');
    }
};

// Apply saved theme on load immediately
(function () {
    const saved = localStorage.getItem('cx-theme') || 'dark';
    if (saved === 'dark') document.documentElement.classList.add('dark');
})();
