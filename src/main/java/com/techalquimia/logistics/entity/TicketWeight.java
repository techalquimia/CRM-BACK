package com.techalquimia.logistics.entity;

import jakarta.persistence.*;
import lombok.*;

import java.util.UUID;

/**
 * Tabla relacionada para pesos de tickets.
 * peso_total = tara + peso_neto
 */
@Entity
@Table(name = "ticket_weights")
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class TicketWeight {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(name = "peso_total")
    private Double pesoTotal;

    @Column(name = "tara")
    private Double tara;

    @Column(name = "peso_neto")
    private Double pesoNeto;

    @OneToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "evidence_id", nullable = false, unique = true)
    private Evidence evidence;
}
