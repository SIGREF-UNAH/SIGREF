import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';

import { Pagination } from './Pagination';

describe('Pagination', () => {
  it('renders the total range and forwards page changes', () => {
    const onChange = vi.fn();

    render(
      <Pagination
        current={2}
        pageSize={10}
        total={35}
        onChange={onChange}
      />,
    );

    expect(screen.getByText('11-20 de 35')).toBeTruthy();
    fireEvent.click(screen.getByTitle('3'));

    expect(onChange).toHaveBeenCalledWith(3, 10);
  });

  it('forwards a numeric page size when the size changes', () => {
    const onChange = vi.fn();

    render(
      <Pagination
        current={1}
        pageSize={10}
        total={35}
        onChange={onChange}
      />,
    );

    fireEvent.mouseDown(screen.getByRole('combobox'));
    fireEvent.click(screen.getByText('20 / page'));

    expect(onChange).toHaveBeenCalledWith(1, 20);
  });

  it('does not allow interaction while disabled', () => {
    const onChange = vi.fn();

    render(
      <Pagination
        current={1}
        pageSize={10}
        total={35}
        onChange={onChange}
        disabled
      />,
    );

    fireEvent.click(screen.getByTitle('2'));

    expect(onChange).not.toHaveBeenCalled();
  });
});
