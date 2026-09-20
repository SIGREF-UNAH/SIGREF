import { describe, expect, it } from 'vitest'
import { buildInvoiceItems, parseReceiptNumber } from './useCreateIncome'

describe('parseReceiptNumber', () => {
  it.each([
    ['0', 0],
    ['-12', -12],
    [' 42 ', 42],
  ])('acepta el entero seguro %s', (value, expected) => {
    expect(parseReceiptNumber(value)).toBe(expected)
  })

  it.each(['1.5', '12abc', 'abc12', '9007199254740992'])(
    'rechaza el valor no entero o inseguro %s',
    (value) => {
      expect(parseReceiptNumber(value)).toBeNull()
    },
  )
})

describe('buildInvoiceItems', () => {
  it('normaliza un servicio individual', () => {
    expect(
      buildInvoiceItems({
        id: 'service-1',
        nombre: 'Consulta',
        precio: 75,
        tipo: 'servicio',
      }),
    ).toEqual([
      {
        serviceId: 'service-1',
        nameService: 'Consulta',
        quantity: 1,
        unitPrice: 75,
      },
    ])
  })

  it('usa el servicio de nivel paquete cuando faltan sus items', () => {
    expect(
      buildInvoiceItems({
        id: 'package-1',
        nombre: 'Paquete básico',
        precio: 100,
        tipo: 'paquete',
      }),
    ).toEqual([
      {
        serviceId: 'package-1',
        nameService: 'Paquete básico',
        quantity: 1,
        unitPrice: 100,
      },
    ])
  })

  it('convierte los servicios de un paquete en items de factura tipados', () => {
    const items = buildInvoiceItems({
      id: 'package-1',
      nombre: 'Paquete básico',
      precio: 100,
      tipo: 'paquete',
      items: [
        {
          id: 'service-1',
          name: 'Consulta',
          price: 75,
        },
        {
          serviceId: 'service-2',
          nameService: 'Examen',
          unitPrice: 25,
          quantity: 2,
        },
      ],
    })

    expect(items).toEqual([
      {
        serviceId: 'service-1',
        nameService: 'Consulta',
        quantity: 1,
        unitPrice: 75,
      },
      {
        serviceId: 'service-2',
        nameService: 'Examen',
        quantity: 2,
        unitPrice: 25,
      },
    ])
  })
})
