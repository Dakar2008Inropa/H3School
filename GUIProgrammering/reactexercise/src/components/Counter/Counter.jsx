import { useEffect, useRef, useState } from "react";
import "./Counter.css";

const STORAGE_KEY = "counter:value";

export const Counter = () => {
    const [count, setCount] = useState(() => {
        try {
            const raw = localStorage.getItem(STORAGE_KEY);
            const parsed = raw == null ? 0 : Number.parseInt(raw, 10);
            return Number.isFinite(parsed) ? parsed : 0;
        } catch (e) {
            console.error("Failed to read counter from localStorage.", e);
            return 0;
        }
    });

    useEffect(() => {
        try {
            localStorage.setItem(STORAGE_KEY, String(count));
        } catch (e) {
            console.error("Failed to write counter to localStorage.", e);
        }
    }, [count]);

    const INITIAL_DELAY = 350;
    const MIN_DELAY = 35;
    const ACCEL_FACTOR = 0.35;

    const pressRef = useRef({
        timer: null,
        delay: INITIAL_DELAY,
        isHolding: false
    });

    function clearPressTimer() {
        if (pressRef.current.timer) {
            clearTimeout(pressRef.current.timer);
            pressRef.current.timer = null;
        }
    }

    function scheduleNext() {
        clearPressTimer();
        const currentDelay = pressRef.current.delay;

        pressRef.current.timer = setTimeout(() => {
            setCount(c => c + 1);

            const nextDelay = Math.max(MIN_DELAY, Math.floor(currentDelay * ACCEL_FACTOR));
            pressRef.current.delay = nextDelay;

            if (pressRef.current.isHolding) {
                scheduleNext();
            }
        }, currentDelay);
    }

    function handlePressStart(e) {
        e.preventDefault();

        pressRef.current.isHolding = true;
        pressRef.current.delay = INITIAL_DELAY;

        setCount(c => c + 1);
        scheduleNext();
    }

    function handlePressStop() {
        pressRef.current.isHolding = false;
        clearPressTimer();
    }

    useEffect(() => {
        return () => clearPressTimer();
    }, []);

    function increment() {
        setCount((c) => c + 1);
    }

    function reset() {
        setCount(0);
    }

    return (
        <section className="counter" aria-label="Counter">
            <div
                className="counter__display"
                aria-live="polite"
                aria-atomic="true"
                title={`Current count: ${count}`}
            >
                {count}
            </div>
            <div className="counter__actions">
                {                                 }
                <button
                    type="button"
                    className="counter__btn counter__btn--inc"
                    onPointerDown={handlePressStart}
                    onPointerUp={handlePressStop}
                    onPointerLeave={handlePressStop}
                    onPointerCancel={handlePressStop}
                >
                    Count It UP!
                </button>
                <button
                    type="button"
                    className="counter__btn counter__btn--reset"
                    onClick={reset}
                >
                    Reset
                </button>
            </div>
        </section>
    );
};