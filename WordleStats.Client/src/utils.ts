export function assertNonNullable<T>(value: T | null | undefined): asserts value is NonNullable<T> {
    if (value === null || value === undefined) {
        throw Error(`Expected non-nullable value, but got ${value}`);
    }
}

export function nameof<T>(name: Extract<keyof T, string>): string {
    return name;
}